using System.Collections.Generic;
using BusJam.Bus;
using BusJam.Data;
using BusJam.Grid;
using BusJam.Input;
using BusJam.Passengers;
using BusJam.UI;
using BusJam.WaitingSlot;
using UnityEngine;

namespace BusJam.Core
{
    public class GameManager : MonoBehaviour
    {
        private const string SAVE_KEY = "CurrentLevel";

        [SerializeField]
        private LevelManager levelManager;
        [SerializeField]
        private InputDetector inputDetector;
        [SerializeField]
        private BusManager busManager;
        [SerializeField]
        private WaitingSlotManager waitingSlotManager;
        [SerializeField]
        private ScreenManager screenManager;
        [SerializeField]
        private BusFactory busFactory;
        [SerializeField]
        private PassengerFactory passengerFactory;
        [SerializeField]
        private CameraConfig cameraConfig;

        private int _levelId;
        private bool _isGameOver;
        private bool _isStarted;
        private LevelData _currentLevelData;
        private TimerController _timerController;

        private void Start()
        {
            _levelId = PlayerPrefs.GetInt(SAVE_KEY, 1);
            _timerController = screenManager.GameplayScreen.TimerController;
            screenManager.StartScreen.OnPlayButtonClicked += HandlePlayClicked;
            screenManager.WinScreen.OnContinueClicked += HandleWinContinueClicked;
            screenManager.LoseScreen.OnRetryClicked += HandleLoseRetryClicked;
            screenManager.LoseScreen.OnCloseClicked += HandleLoseCloseClicked;
            ShowStartScreen();
        }

        private void ShowStartScreen()
        {
            var levelAsset = Resources.Load<TextAsset>("Levels/Level_" + _levelId);
            if (levelAsset == null)
            {
                screenManager.StartScreen.SetCompleted();
                screenManager.StartScreen.Show();
                return;
            }
            screenManager.StartScreen.SetLevelText(_levelId);
            screenManager.StartScreen.Show();
        }

        private void HandlePlayClicked()
        {
            screenManager.FadeTransition(
                onBlack: () =>
                {
                    screenManager.StartScreen.Hide();
                    screenManager.GameplayScreen.Show();
                    LoadLevel(_levelId);
                });
        }

        private void HandleWinContinueClicked()
        {
            screenManager.FadeTransition(
                onBlack: () =>
                {
                    screenManager.WinScreen.Hide();
                    CleanupLevel();
                    screenManager.GameplayScreen.Hide();
                    ShowStartScreen();
                });
        }

        private void HandleLoseRetryClicked()
        {
            screenManager.FadeTransition(
                onBlack: () =>
                {
                    screenManager.LoseScreen.Hide();
                    CleanupLevel();
                    LoadLevel(_levelId);
                });
        }

        private void HandleLoseCloseClicked()
        {
            screenManager.FadeTransition(
                onBlack: () =>
                {
                    screenManager.LoseScreen.Hide();
                    CleanupLevel();
                    screenManager.GameplayScreen.Hide();
                    ShowStartScreen();
                });
        }

        private void LoadLevel(int id)
        {
            _isGameOver = false;
            _isStarted = false;
            var textAsset = Resources.Load<TextAsset>("Levels/Level_" + id);
            _currentLevelData = JsonUtility.FromJson<LevelData>(textAsset.text);
            levelManager.CreateLevel(_currentLevelData);
            Camera.main.transform.position = cameraConfig.GetPosition(
                _currentLevelData.cameraPreset);
            busManager.CreateBuses(_currentLevelData.buses);
            waitingSlotManager.CreateCells(_currentLevelData.waitingCellCount);
            screenManager.GameplayScreen.SetLevelText(id);
            _timerController.SetTimer(_currentLevelData.timeLimit);
            screenManager.GameplayScreen.SetTimerText(_currentLevelData.timeLimit);
            _timerController.OnTimeUp += HandleTimeUp;
            inputDetector.OnCellTapped += HandleCellTapped;
            busManager.OnBusChanged += HandleBusChanged;
            levelManager.CheckTunnels();
            levelManager.UpdateMovableStates();
        }

        public void ReloadLevel(int levelId)
        {
            CleanupLevel();
            _levelId = levelId;
            screenManager.GameplayScreen.Show();
            LoadLevel(_levelId);
        }

        private void CleanupLevel()
        {
            inputDetector.OnCellTapped -= HandleCellTapped;
            busManager.OnBusChanged -= HandleBusChanged;
            _timerController.OnTimeUp -= HandleTimeUp;
            _timerController.StopTimer();
            inputDetector.SetEnabled(false);
            busFactory.ReturnAll();
            passengerFactory.ReturnAll();
            levelManager.Cleanup();
            waitingSlotManager.Cleanup();
            busManager.Cleanup();
        }

        private void HandleCellTapped(Cell cell)
        {
            if (_isGameOver)
            {
                return;
            }
            MoveToExit(cell);
        }

        private void MoveToExit(Cell cell)
        {
            var passenger = cell.Passenger;
            if (passenger == null)
            {
                return;
            }
            if (!_isStarted)
            {
                HandleGameStarted();
            }

            if (!cell.IsMovable)
            {
                return;
            }

            var path = cell.CachedPathList;
            cell.ClearPassenger();
           
            passenger.MoveAlongPath(path, () =>
            {
                HandleReachedExit(passenger);
            });
            
            levelManager.CheckTunnels();
            levelManager.UpdateMovableStates();
        }

        private void HandleReachedExit(Passenger passenger)
        {
            var activeBus = busManager.ActiveBus;
            if (activeBus != null && activeBus.IsArrived && activeBus.ColorType == passenger.Color)
            {
                var seat = activeBus.GetEmptySeat();
                if (seat != null)
                {
                    seat.SetPassenger(passenger);
                    SendToBus(passenger, activeBus, seat);
                    return;
                }
            }
            SendToWaiting(passenger);
        }

        private void SendToBus(Passenger passenger, Bus.Bus bus, BusSeat seat)
        {
            bus.OpenDoors();
            passenger.MoveToBus(bus.EntryPoint, () =>
            {
                passenger.MoveToSeat(seat.transform, () =>
                {
                    seat.Arrive();
                    passenger.transform.SetParent(seat.transform);
                    HandlePassengerSeated();
                });
            });
        }

        private void SendToWaiting(Passenger passenger)
        {
            if (_isGameOver)
            {
                return;
            }
            var waitingCell = waitingSlotManager.GetEmptyCell();
            if (waitingCell == null)
            {
                HandleGameFailed(GameEndReason.OutOfTiles);
                return;
            }
            waitingCell.SetPassenger(passenger);
            passenger.MoveToCell(waitingCell.transform, () =>
            {
                waitingCell.Arrive();
                TrySendWaitingToBus();
            });
        }

        private void HandlePassengerSeated()
        {
            if (_isGameOver)
            {
                return;
            }
            var activeBus = busManager.ActiveBus;
            if (activeBus == null)
            {
                return;
            }
            activeBus.CloseDoors();
            if (activeBus.IsFull())
            {
                busManager.NextBus();
            }
            TrySendWaitingToBus();
        }

        private void HandleBusChanged()
        {
            if (_isGameOver)
            {
                return;
            }
            if (busManager.ActiveBus == null)
            {
                HandleWin();
                return;
            }
            inputDetector.SetEnabled(true);
            levelManager.UpdateMovableStates();
            TrySendWaitingToBus();
        }

        private void TrySendWaitingToBus()
        {
            var activeBus = busManager.ActiveBus;
            if (activeBus == null || !activeBus.IsArrived)
            {
                return;
            }
            while (activeBus.GetEmptySeat() != null)
            {
                var waitingCell = waitingSlotManager.GetMatchingCell(activeBus.ColorType);
                if (waitingCell == null)
                {
                    break;
                }
                var seat = activeBus.GetEmptySeat();
                var passenger = waitingCell.Passenger;
                seat.SetPassenger(passenger);
                waitingCell.Clear();
                SendToBus(passenger, activeBus, seat);
            }
        }

        private void HandleGameStarted()
        {
            _isStarted = true;
            _timerController.StartTimer();
        }

        private void HandleGameEnded()
        {
            _isGameOver = true;
            inputDetector.SetEnabled(false);
            _timerController.StopTimer();
        }

        private void HandleTimeUp()
        {
            if (_isGameOver)
            {
                return;
            }
            screenManager.GameplayScreen.SetTimerText(0); 
            HandleGameFailed(GameEndReason.TimeUp);
        }

        private void HandleWin()
        {
            HandleGameEnded();
            _levelId++;
            PlayerPrefs.SetInt(SAVE_KEY, _levelId);
            screenManager.WinScreen.FadeIn();
        }

        private void HandleGameFailed(GameEndReason reason)
        {
            HandleGameEnded();
            screenManager.LoseScreen.ShowLose(reason);
        }
    }
}