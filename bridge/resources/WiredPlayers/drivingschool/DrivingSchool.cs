using GTANetworkAPI;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.model;
using System.Collections.Generic;
using System.Threading;
using System;

namespace WiredPlayers.drivingschool
{
    public class DrivingSchool : Script
    {
        private static Dictionary<int, Timer> drivingSchoolTimerList = new Dictionary<int, Timer>();


        public static void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            if (drivingSchoolTimerList.TryGetValue(player.Value, out Timer drivingSchoolTimer) == true)
            {
                // We remove the timer
                drivingSchoolTimer.Dispose();
                drivingSchoolTimerList.Remove(player.Value);
            }
        }

        private void OnDrivingTimer(object playerObject)
        {
            // We get the player and his vehicle
            Client player = (Client)playerObject;
            Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE);

            // We finish the exam
            FinishDrivingExam(player, vehicle);

            // Deleting timer from the list
            if (drivingSchoolTimerList.TryGetValue(player.Value, out Timer drivingSchoolTimer) == true)
            {
                drivingSchoolTimer.Dispose();
                drivingSchoolTimerList.Remove(player.Value);
            }

            // Confirmation message sent to the player
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_LICENSE_FAILED_NOT_IN_VEHICLE);
        }

        private void FinishDrivingExam(Client player, Vehicle vehicle)
        {
            // Vehicle reseting
            NAPI.Vehicle.RepairVehicle(vehicle);
            NAPI.Entity.SetEntityPosition(vehicle, NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION));
            NAPI.Entity.SetEntityRotation(vehicle, NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ROTATION));

            // Checkpoint delete
            if(NAPI.Vehicle.GetVehicleDriver(vehicle) == player)
            {
                Checkpoint licenseCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE);
                NAPI.Entity.DeleteEntity(licenseCheckpoint);
                NAPI.ClientEvent.TriggerClientEvent(player, "deleteLicenseCheckpoint");
            }

            // Entity data cleanup
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_VEHICLE);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DRIVING_EXAM);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT);

            // Remove player from vehicle
            NAPI.Player.WarpPlayerOutOfVehicle(player);
        }

        public static int GetPlayerLicenseStatus(Client player, int license)
        {
            String playerLicenses = NAPI.Data.GetEntityData(player, EntityData.PLAYER_LICENSES);
            String[] licenses = playerLicenses.Split(',');
            return Int32.Parse(licenses[license]);
        }

        public static void SetPlayerLicense(Client player, int license, int value)
        {
            // We get player licenses
            String playerLicenses = NAPI.Data.GetEntityData(player, EntityData.PLAYER_LICENSES);
            String[] licenses = playerLicenses.Split(',');

            // Changing license status
            licenses[license] = value.ToString();
            playerLicenses = String.Join(",", licenses);

            // Save the new licenses
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_LICENSES, playerLicenses);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Client player, Vehicle vehicle, sbyte seatId)
        {
            if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.FACTION_DRIVING_SCHOOL)
            {
                VehicleHash vehicleHash = (VehicleHash)NAPI.Entity.GetEntityModel(vehicle);
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DRIVING_EXAM) && NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_EXAM) == Constants.CAR_DRIVING_PRACTICE)
                {
                    // We check the class of the vehicle
                    if (NAPI.Vehicle.GetVehicleClass(vehicleHash) == Constants.VEHICLE_CLASS_SEDANS)
                    {
                        int checkPoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT);
                        if (drivingSchoolTimerList.TryGetValue(player.Value, out Timer drivingSchoolTimer) == true)
                        {
                            drivingSchoolTimer.Dispose();
                            drivingSchoolTimerList.Remove(player.Value);
                        }
                        Checkpoint newCheckpoint = NAPI.Checkpoint.CreateCheckpoint(0, Constants.CAR_LICENSE_CHECKPOINTS[checkPoint], Constants.CAR_LICENSE_CHECKPOINTS[checkPoint + 1], 2.5f, new Color(198, 40, 40, 200));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE, newCheckpoint);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_VEHICLE, vehicle);

                        // We place a mark on the map
                        NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", Constants.CAR_LICENSE_CHECKPOINTS[checkPoint]);
                    }
                    else
                    {
                        NAPI.Player.WarpPlayerOutOfVehicle(player);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_DRIVING_NOT_SUITABLE);
                    }
                }
                else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DRIVING_EXAM) && NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_EXAM) == Constants.MOTORCYCLE_DRIVING_PRACTICE)
                {
                    // We check the class of the vehicle
                    if (NAPI.Vehicle.GetVehicleClass(vehicleHash) == Constants.VEHICLE_CLASS_MOTORCYCLES)
                    {
                        int checkPoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT);
                        if (drivingSchoolTimerList.TryGetValue(player.Value, out Timer drivingSchoolTimer) == true)
                        {
                            drivingSchoolTimer.Dispose();
                            drivingSchoolTimerList.Remove(player.Value);
                        }
                        Checkpoint newCheckpoint = NAPI.Checkpoint.CreateCheckpoint(0, Constants.BIKE_LICENSE_CHECKPOINTS[checkPoint], Constants.BIKE_LICENSE_CHECKPOINTS[checkPoint + 1], 2.5f, new Color(198, 40, 40, 200));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE, newCheckpoint);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_VEHICLE, vehicle);

                        // We place a mark on the map
                        NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", Constants.BIKE_LICENSE_CHECKPOINTS[checkPoint]);
                    }
                    else
                    {
                        NAPI.Player.WarpPlayerOutOfVehicle(player);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_DRIVING_NOT_SUITABLE);
                    }
                }
                else
                {
                    NAPI.Player.WarpPlayerOutOfVehicle(player);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_CAR_PRACTICE);
                }
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Client player, Vehicle vehicle)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DRIVING_EXAM) && NAPI.Data.HasEntityData(player, EntityData.PLAYER_VEHICLE) == true)
            {
                // Checking if is a valid vehicle
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE) == vehicle && NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.FACTION_DRIVING_SCHOOL)
                {
                    String warn = String.Format(Messages.INF_LICENSE_VEHICLE_EXIT, 15);
                    Checkpoint playerDrivingCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE);
                    NAPI.Entity.DeleteEntity(playerDrivingCheckpoint);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + warn);

                    // Removing the checkpoint marker
                    NAPI.ClientEvent.TriggerClientEvent(player, "deleteLicenseCheckpoint");

                    // When the timer finishes, the exam will be failed
                    Timer drivingSchoolTimer = new Timer(OnDrivingTimer, player, 15000, Timeout.Infinite);
                    drivingSchoolTimerList.Add(player.Value, drivingSchoolTimer);
                }
            }
        }

        [ServerEvent(Event.PlayerEnterCheckpoint)]
        public void OnPlayerEnterCheckpoint(Checkpoint checkpoint, Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE) && NAPI.Data.HasEntityData(player, EntityData.PLAYER_DRIVING_EXAM) == true)
            {
                if (NAPI.Player.IsPlayerInAnyVehicle(player) == true && NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_EXAM) == Constants.CAR_DRIVING_PRACTICE)
                {
                    Vehicle vehicle = NAPI.Player.GetPlayerVehicle(player);
                    if (checkpoint == NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE) && NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.FACTION_DRIVING_SCHOOL)
                    {
                        Checkpoint currentCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE);
                        int checkPoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT);

                        if (checkPoint < Constants.CAR_LICENSE_CHECKPOINTS.Count - 2)
                        {
                            NAPI.Entity.SetEntityPosition(currentCheckpoint, Constants.CAR_LICENSE_CHECKPOINTS[checkPoint + 1]);
                            NAPI.Checkpoint.SetCheckpointDirection(currentCheckpoint, Constants.CAR_LICENSE_CHECKPOINTS[checkPoint + 2]);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, checkPoint + 1);

                            // We place a mark on the map
                            NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", currentCheckpoint.Position);
                        }
                        else if (checkPoint == Constants.CAR_LICENSE_CHECKPOINTS.Count - 2)
                        {
                            NAPI.Entity.SetEntityPosition(currentCheckpoint, Constants.CAR_LICENSE_CHECKPOINTS[checkPoint + 1]);
                            NAPI.Checkpoint.SetCheckpointDirection(currentCheckpoint, NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION));
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, checkPoint + 1);

                            // We place a mark on the map
                            NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", currentCheckpoint.Position);
                        }
                        else if (checkPoint == Constants.CAR_LICENSE_CHECKPOINTS.Count - 1)
                        {
                            Vector3 lastCheckPointPosition = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION);
                            NAPI.Entity.SetEntityPosition(currentCheckpoint, lastCheckPointPosition);
                            NAPI.Entity.SetEntityModel(currentCheckpoint, (int)CheckpointType.Checkerboard);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, checkPoint + 1);

                            // We place a mark on the map
                            NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", currentCheckpoint.Position);
                        }
                        else
                        {
                            // Exam finished
                            FinishDrivingExam(player, vehicle);

                            // We add points to the license
                            SetPlayerLicense(player, Constants.LICENSE_CAR, 12);

                            // Confirmation message sent to the player
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + Messages.SUC_LICENSE_DRIVE_PASSED);
                        }
                    }
                }
                else if (NAPI.Player.IsPlayerInAnyVehicle(player) == true && NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_EXAM) == Constants.MOTORCYCLE_DRIVING_PRACTICE)
                {
                    Vehicle vehicle = NAPI.Player.GetPlayerVehicle(player);
                    if (checkpoint == NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE) && NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.FACTION_DRIVING_SCHOOL)
                    {
                        Checkpoint currentCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE);
                        int checkPoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT);

                        if (checkPoint < Constants.BIKE_LICENSE_CHECKPOINTS.Count - 2)
                        {
                            NAPI.Entity.SetEntityPosition(currentCheckpoint, Constants.BIKE_LICENSE_CHECKPOINTS[checkPoint + 1]);
                            NAPI.Checkpoint.SetCheckpointDirection(currentCheckpoint, Constants.BIKE_LICENSE_CHECKPOINTS[checkPoint + 2]);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, checkPoint + 1);

                            // We place a mark on the map
                            NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", currentCheckpoint.Position);
                        }
                        else if (checkPoint == Constants.BIKE_LICENSE_CHECKPOINTS.Count - 2)
                        {
                            NAPI.Entity.SetEntityPosition(currentCheckpoint, Constants.BIKE_LICENSE_CHECKPOINTS[checkPoint + 1]);
                            NAPI.Checkpoint.SetCheckpointDirection(currentCheckpoint, NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION));
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, checkPoint + 1);

                            // We place a mark on the map
                            NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", currentCheckpoint.Position);
                        }
                        else if (checkPoint == Constants.BIKE_LICENSE_CHECKPOINTS.Count - 1)
                        {
                            Vector3 lastCheckPointPosition = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION);
                            NAPI.Entity.SetEntityPosition(currentCheckpoint, lastCheckPointPosition);
                            NAPI.Entity.SetEntityModel(currentCheckpoint, (int)CheckpointType.Checkerboard);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, checkPoint + 1);

                            // We place a mark on the map
                            NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", currentCheckpoint.Position);
                        }
                        else
                        {
                            // Exam finished
                            FinishDrivingExam(player, vehicle);

                            // We add points to the license
                            SetPlayerLicense(player, Constants.LICENSE_MOTORCYCLE, 12);

                            // Confirmation message sent to the player
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + Messages.SUC_LICENSE_DRIVE_PASSED);
                        }
                    }
                }
            }
        }

        [ServerEvent(Event.VehicleDamage)]
        public void OnVehicleDamage(Vehicle vehicle, float lossFirst, float lossSecond)
        {
            Client player = NAPI.Vehicle.GetVehicleDriver(vehicle);
            if (player != null && NAPI.Data.HasEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE) && NAPI.Data.HasEntityData(player, EntityData.PLAYER_DRIVING_EXAM) == true)
            {
                float currentHealth = NAPI.Vehicle.GetVehicleHealth(vehicle);

                if (lossFirst - currentHealth > 5.0f)
                {
                    // Exam finished
                    FinishDrivingExam(player, vehicle);

                    // Inform the player about his failure
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_LICENSE_DRIVE_FAILED);
                }
            }
        }

        [ServerEvent(Event.Update)]
        public void OnUpdate()
        {
            foreach (Client player in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) && NAPI.Data.HasEntityData(player, EntityData.PLAYER_DRIVING_EXAM) == true)
                {
                    // Check if is driving a vehicle
                    if (NAPI.Player.IsPlayerInAnyVehicle(player) && NAPI.Player.GetPlayerVehicleSeat(player) == (int)VehicleSeat.Driver)
                    {
                        Vehicle vehicle = NAPI.Player.GetPlayerVehicle(player);
                        if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.FACTION_DRIVING_SCHOOL)
                        {
                            Vector3 velocity = NAPI.Entity.GetEntityVelocity(vehicle);
                            double speed = Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y + velocity.Z * velocity.Z);
                            if (Math.Round(speed * 3.6f) > Constants.MAX_DRIVING_VEHICLE)
                            {
                                // Exam finished
                                FinishDrivingExam(player, vehicle);

                                // Inform the player about his failure
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_LICENSE_DRIVE_FAILED);
                            }
                        }
                    }
                }
            }
        }

        [RemoteEvent("checkAnswer")]
        public void CheckAnswerEvent(Client player, int answer)
        {
            if (Database.CheckAnswerCorrect(answer) == true)
            {
                // We add the correct answer
                int nextQuestion = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_LICENSE_QUESTION) + 1;

                if (nextQuestion < Constants.MAX_LICENSE_QUESTIONS)
                {
                    // Go for the next question
                    NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_LICENSE_QUESTION, nextQuestion);
                    NAPI.ClientEvent.TriggerClientEvent(player, "getNextTestQuestion");
                }
                else
                {
                    // Player passed the exam
                    int license = NAPI.Data.GetEntityData(player, EntityData.PLAYER_LICENSE_TYPE);
                    SetPlayerLicense(player, license, 0);

                    // Reset the entity data
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_LICENSE_TYPE);
                    NAPI.Data.ResetEntitySharedData(player, EntityData.PLAYER_LICENSE_QUESTION);

                    // Send the message to the player
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + Messages.SUC_LICENSE_EXAM_PASSED);

                    // Exam window close
                    NAPI.ClientEvent.TriggerClientEvent(player, "finishLicenseExam");
                }
            }
            else
            {
                // Player failed the exam
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_LICENSE_EXAM_FAILED);

                // Reset the entity data
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_LICENSE_TYPE);
                NAPI.Data.ResetEntitySharedData(player, EntityData.PLAYER_LICENSE_QUESTION);

                // Exam window close
                NAPI.ClientEvent.TriggerClientEvent(player, "finishLicenseExam");
            }
        }

        [Command(Commands.COMMAND_DRIVING_SCHOOL, Messages.GEN_DRIVING_SCHOOL_COMMAND)]
        public void DrivingSchoolCommand(Client player, String type)
        {
            int licenseStatus = 0;
            foreach (InteriorModel interior in Constants.INTERIOR_LIST)
            {
                if (interior.captionMessage == Messages.GEN_DRIVING_SCHOOL && player.Position.DistanceTo(interior.entrancePosition) < 2.5f)
                {
                    List<TestModel> questions = new List<TestModel>();
                    List<TestModel> answers = new List<TestModel>();

                    // Get the player's money
                    int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                    
                    switch (type.ToLower())
                    {
                        case Commands.ARGUMENT_CAR:
                            // Check for the status if the license
                            licenseStatus = GetPlayerLicenseStatus(player, Constants.LICENSE_CAR);

                            switch (licenseStatus)
                            {
                                case -1:
                                    // Check if the player has enough money
                                    if (money >= Constants.PRICE_DRIVING_THEORICAL)
                                    {
                                        // Add the questions
                                        questions = Database.GetRandomQuestions(Constants.LICENSE_CAR + 1);
                                        foreach (TestModel question in questions)
                                        {
                                            answers.AddRange(Database.GetQuestionAnswers(question.id));
                                        }
                                        
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_LICENSE_TYPE, Constants.LICENSE_CAR);
                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_LICENSE_QUESTION, 0);

                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - Constants.PRICE_DRIVING_THEORICAL);

                                        // Start the exam
                                        NAPI.ClientEvent.TriggerClientEvent(player, "startLicenseExam", NAPI.Util.ToJson(questions), NAPI.Util.ToJson(answers));
                                    }
                                    else
                                    {
                                        String message = String.Format(Messages.ERR_DRIVING_SCHOOL_MONEY, Constants.PRICE_DRIVING_THEORICAL);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                                    }
                                    break;
                                case 0:
                                    // Check if the player has enough money
                                    if (money >= Constants.PRICE_DRIVING_PRACTICAL)
                                    {
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_LICENSE_TYPE, Constants.LICENSE_CAR);
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_EXAM, Constants.CAR_DRIVING_PRACTICE);
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, 0);

                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - Constants.PRICE_DRIVING_PRACTICAL);

                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_ENTER_LICENSE_CAR_VEHICLE);
                                    }
                                    else
                                    {
                                        String message = String.Format(Messages.ERR_DRIVING_SCHOOL_MONEY, Constants.PRICE_DRIVING_PRACTICAL);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                                    }
                                    break;
                                default:
                                    // License up to date
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_ALREADY_LICENSE);
                                    break;
                            }
                            break;
                        case Commands.ARGUMENT_MOTORCYCLE:
                            // Check for the status if the license
                            licenseStatus = GetPlayerLicenseStatus(player, Constants.LICENSE_MOTORCYCLE);

                            switch (licenseStatus)
                            {
                                case -1:
                                    // Check if the player has enough money
                                    if (money >= Constants.PRICE_DRIVING_THEORICAL)
                                    {
                                        // Add the questions
                                        questions = Database.GetRandomQuestions(Constants.LICENSE_MOTORCYCLE + 1);
                                        foreach (TestModel question in questions)
                                        {
                                            answers.AddRange(Database.GetQuestionAnswers(question.id));
                                        }
                                        
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_LICENSE_TYPE, Constants.LICENSE_MOTORCYCLE);
                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_LICENSE_QUESTION, 0);

                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - Constants.PRICE_DRIVING_THEORICAL);

                                        // Start the exam
                                        NAPI.ClientEvent.TriggerClientEvent(player, "startLicenseExam", NAPI.Util.ToJson(questions), NAPI.Util.ToJson(answers));
                                    }
                                    else
                                    {
                                        String message = String.Format(Messages.ERR_DRIVING_SCHOOL_MONEY, Constants.PRICE_DRIVING_THEORICAL);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                                    }
                                    break;
                                case 0:
                                    // Check if the player has enough money
                                    if (money >= Constants.PRICE_DRIVING_PRACTICAL)
                                    {
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_LICENSE_TYPE, Constants.LICENSE_MOTORCYCLE);
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_EXAM, Constants.MOTORCYCLE_DRIVING_PRACTICE);
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, 0);

                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - Constants.PRICE_DRIVING_PRACTICAL);

                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_ENTER_LICENSE_BIKE_VEHICLE);
                                    }
                                    else
                                    {
                                        String message = String.Format(Messages.ERR_DRIVING_SCHOOL_MONEY, Constants.PRICE_DRIVING_PRACTICAL);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                                    }
                                    break;
                                default:
                                    // License up to date
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_ALREADY_LICENSE);
                                    break;
                            }
                            break;
                        default:
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_DRIVING_SCHOOL_COMMAND);
                            break;
                    }
                    return;
                }
            }

            // Player's not in the driving school
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_DRIVING_SCHOOL);
        }

        [Command(Commands.COMMAND_LICENSES)]
        public void LicensesCommand(Client player)
        {
            int currentLicense = 0;
            String playerLicenses = NAPI.Data.GetEntityData(player, EntityData.PLAYER_LICENSES);
            String[] playerLicensesArray = playerLicenses.Split(',');
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_LICENSE_LIST);
            foreach (String license in playerLicensesArray)
            {
                int currentLicenseStatus = Int32.Parse(license);
                switch (currentLicense)
                {
                    case Constants.LICENSE_CAR:
                        switch (currentLicenseStatus)
                        {
                            case -1:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.INF_CAR_LICENSE_NOT_AVAILABLE);
                                break;
                            case 0:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.INF_CAR_LICENSE_PRACTICAL_PENDING);
                                break;
                            default:
                                String message = String.Format(Messages.INF_CAR_LICENSE_POINTS + currentLicenseStatus);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + message);
                                break;
                        }
                        break;
                    case Constants.LICENSE_MOTORCYCLE:
                        switch (currentLicenseStatus)
                        {
                            case -1:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.INF_MOTORCYCLE_LICENSE_NOT_AVAILABLE);
                                break;
                            case 0:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.INF_MOTORCYCLE_LICENSE_PRACTICAL_PENDING);
                                break;
                            default:
                                String message = String.Format(Messages.INF_MOTORCYCLE_LICENSE_POINTS + currentLicenseStatus);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + message);
                                break;
                        }
                        break;
                    case Constants.LICENSE_TAXI:
                        if (currentLicenseStatus == -1)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.INF_TAXI_LICENSE_NOT_AVAILABLE);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.INF_TAXI_LICENSE_UP_TO_DATE);
                        }
                        break;
                }
                currentLicense++;
            }
        }
    }
}