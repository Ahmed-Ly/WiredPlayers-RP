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
                // Eliminamos el timer
                drivingSchoolTimer.Dispose();
                drivingSchoolTimerList.Remove(player.Value);
            }
        }

        private void OnDrivingTimer(object playerObject)
        {
            try
            {
                // Obtenemos el jugador y su vehículo
                Client player = (Client)playerObject;
                Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE);

                // Finalizamos el examen
                FinishDrivingExam(player, vehicle);

                // Borramos el timer de la lista
                if (drivingSchoolTimerList.TryGetValue(player.Value, out Timer drivingSchoolTimer) == true)
                {
                    drivingSchoolTimer.Dispose();
                    drivingSchoolTimerList.Remove(player.Value);
                }

                // Enviamos un mensaje al jugador
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_LICENSE_FAILED_NOT_IN_VEHICLE);
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION OnDrivingTimer] " + ex.Message);
                NAPI.Util.ConsoleOutput("[EXCEPTION OnDrivingTimer] " + ex.StackTrace);
            }
        }

        private void FinishDrivingExam(Client player, Vehicle vehicle)
        {
            // Reseteamos el vehículo
            NAPI.Vehicle.RepairVehicle(vehicle);
            NAPI.Entity.SetEntityPosition(vehicle, NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION));
            NAPI.Entity.SetEntityRotation(vehicle, NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ROTATION));

            // Eliminamos el checkpoint
            Checkpoint licenseCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE);
            NAPI.Entity.DeleteEntity(licenseCheckpoint);
            NAPI.ClientEvent.TriggerClientEvent(player, "deleteLicenseCheckpoint");

            // Reseteamos las variables
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_VEHICLE);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DRIVING_EXAM);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT);

            // Sacamos al jugador del vehículo
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
            // Obtenemos las licencias
            String playerLicenses = NAPI.Data.GetEntityData(player, EntityData.PLAYER_LICENSES);
            String[] licenses = playerLicenses.Split(',');

            // Cambiamos el estado o puntos de la licencia
            licenses[license] = value.ToString();
            playerLicenses = String.Join(",", licenses);

            // Guardamos el estado de las licencias
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
                    // Miramos si es un coche
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

                        // Marcamos el punto en el mapa
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
                    // Miramos si es una moto
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

                        // Marcamos el punto en el mapa
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
                // Está subido en un vehículo de autoescuela
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE) == vehicle && NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.FACTION_DRIVING_SCHOOL)
                {
                    String warn = String.Format(Messages.INF_LICENSE_VEHICLE_EXIT, 15);
                    Checkpoint playerDrivingCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE);
                    NAPI.Entity.DeleteEntity(playerDrivingCheckpoint);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + warn);

                    // Eliminamos la marca del mapa
                    NAPI.ClientEvent.TriggerClientEvent(player, "deleteLicenseCheckpoint");

                    // Creamos el timer para volver a subirse
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
                            currentCheckpoint.Position = Constants.CAR_LICENSE_CHECKPOINTS[checkPoint + 1];
                            currentCheckpoint.Direction = Constants.CAR_LICENSE_CHECKPOINTS[checkPoint + 2];
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, checkPoint + 1);

                            // Marcamos el punto en el mapa
                            NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", currentCheckpoint.Position);
                        }
                        else if (checkPoint == Constants.CAR_LICENSE_CHECKPOINTS.Count - 2)
                        {
                            currentCheckpoint.Position = Constants.CAR_LICENSE_CHECKPOINTS[checkPoint + 1];
                            currentCheckpoint.Direction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, checkPoint + 1);

                            // Marcamos el punto en el mapa
                            NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", currentCheckpoint.Position);
                        }
                        else if (checkPoint == Constants.CAR_LICENSE_CHECKPOINTS.Count - 1)
                        {
                            Vector3 lastCheckPointPosition = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION);
                            currentCheckpoint.Position = lastCheckPointPosition;
                            NAPI.Entity.SetEntityModel(currentCheckpoint, 4);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, checkPoint + 1);

                            // Marcamos el punto en el mapa
                            NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", currentCheckpoint.Position);
                        }
                        else
                        {
                            // Finalizamos el examen
                            FinishDrivingExam(player, vehicle);

                            // Añadimos los puntos a la licencia
                            SetPlayerLicense(player, Constants.LICENSE_CAR, 12);

                            // Avisamos al jugador de que ha aprobado
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
                            currentCheckpoint.Position = Constants.BIKE_LICENSE_CHECKPOINTS[checkPoint + 1];
                            currentCheckpoint.Direction = Constants.BIKE_LICENSE_CHECKPOINTS[checkPoint + 2];
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, checkPoint + 1);

                            // Marcamos el punto en el mapa
                            NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", currentCheckpoint.Position);
                        }
                        else if (checkPoint == Constants.BIKE_LICENSE_CHECKPOINTS.Count - 2)
                        {
                            currentCheckpoint.Position = Constants.BIKE_LICENSE_CHECKPOINTS[checkPoint + 1];
                            currentCheckpoint.Direction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, checkPoint + 1);

                            // Marcamos el punto en el mapa
                            NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", currentCheckpoint.Position);
                        }
                        else if (checkPoint == Constants.BIKE_LICENSE_CHECKPOINTS.Count - 1)
                        {
                            Vector3 lastCheckPointPosition = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION);
                            currentCheckpoint.Position = lastCheckPointPosition;
                            NAPI.Entity.SetEntityModel(currentCheckpoint, 4);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, checkPoint + 1);

                            // Marcamos el punto en el mapa
                            NAPI.ClientEvent.TriggerClientEvent(player, "showLicenseCheckpoint", currentCheckpoint.Position);
                        }
                        else
                        {
                            // Finalizamos el examen
                            FinishDrivingExam(player, vehicle);

                            // Añadimos los puntos a la licencia
                            SetPlayerLicense(player, Constants.LICENSE_MOTORCYCLE, 12);

                            // Avisamos al jugador de que ha aprobado
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
                // Calculamos el estado actual de daños
                float currentHealth = NAPI.Vehicle.GetVehicleHealth(vehicle);

                if (lossFirst - currentHealth > 5.0f)
                {
                    // Finalizamos el examen
                    FinishDrivingExam(player, vehicle);

                    // Avisamos al jugador del suspenso
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
                    // Comprobamos si está conduciendo un vehículo
                    if (NAPI.Player.IsPlayerInAnyVehicle(player) && NAPI.Player.GetPlayerVehicleSeat(player) == (int)VehicleSeat.Driver)
                    {
                        Vehicle vehicle = NAPI.Player.GetPlayerVehicle(player);
                        if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.FACTION_DRIVING_SCHOOL)
                        {
                            Vector3 velocity = NAPI.Entity.GetEntityVelocity(vehicle);
                            double speed = Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y + velocity.Z * velocity.Z);
                            if (Math.Round(speed * 3.6f) > Constants.MAX_DRIVING_VEHICLE)
                            {
                                // Finalizamos el examen
                                FinishDrivingExam(player, vehicle);

                                // Avisamos al jugador del suspenso
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
                // Incrementamos el número de preguntas
                int nextQuestion = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_LICENSE_QUESTION) + 1;

                if (nextQuestion < Constants.MAX_LICENSE_QUESTIONS)
                {
                    // Aún quedan más preguntas, mostramos la siguiente
                    NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_LICENSE_QUESTION, nextQuestion);
                    NAPI.ClientEvent.TriggerClientEvent(player, "getNextTestQuestion");
                }
                else
                {
                    // Ha aprobado el examen
                    int license = NAPI.Data.GetEntityData(player, EntityData.PLAYER_LICENSE_TYPE);
                    SetPlayerLicense(player, license, 0);

                    // Restablecemos las variables
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_LICENSE_TYPE);
                    NAPI.Data.ResetEntitySharedData(player, EntityData.PLAYER_LICENSE_QUESTION);

                    // Mandamos el mensaje
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + Messages.SUC_LICENSE_EXAM_PASSED);

                    // Cerramos la ventana del examen
                    NAPI.ClientEvent.TriggerClientEvent(player, "finishLicenseExam");
                }
            }
            else
            {
                // Ha fallado la pregunta
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_LICENSE_EXAM_FAILED);

                // Restablecemos las variables
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_LICENSE_TYPE);
                NAPI.Data.ResetEntitySharedData(player, EntityData.PLAYER_LICENSE_QUESTION);

                // Cerramos la ventana del examen
                NAPI.ClientEvent.TriggerClientEvent(player, "finishLicenseExam");
            }
        }

        [Command("autoescuela", Messages.GEN_DRIVING_SCHOOL_COMMAND)]
        public void AutoescuelaCommand(Client player, String type)
        {
            int licenseStatus = 0;
            foreach (InteriorModel interior in Constants.INTERIOR_LIST)
            {
                if (interior.captionMessage == "Autoescuela" && player.Position.DistanceTo(interior.entrancePosition) < 2.5f)
                {
                    // Inicializamos las listas de preguntas y respuestas
                    List<TestModel> questions = new List<TestModel>();
                    List<TestModel> answers = new List<TestModel>();

                    // Obtenemos el dinero del jugador
                    int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);

                    // Miramos la opción elegida
                    switch (type.ToLower())
                    {
                        case "turismo":
                            // Miramos el estado de la licencia de turismos
                            licenseStatus = GetPlayerLicenseStatus(player, Constants.LICENSE_CAR);
                            switch (licenseStatus)
                            {
                                case -1:
                                    // Comprobamos si tiene dinero suficiente
                                    if (money >= Constants.PRICE_DRIVING_THEORICAL)
                                    {
                                        // No ha aprobado ningún examen
                                        questions = Database.GetRandomQuestions(Constants.LICENSE_CAR + 1);
                                        foreach (TestModel question in questions)
                                        {
                                            answers.AddRange(Database.GetQuestionAnswers(question.id));
                                        }

                                        // Añadimos el tipo de licencia y cobramos
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_LICENSE_TYPE, Constants.LICENSE_CAR);
                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_LICENSE_QUESTION, 0);
                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - Constants.PRICE_DRIVING_THEORICAL);

                                        // Mostramos la pantalla del examen teórico
                                        NAPI.ClientEvent.TriggerClientEvent(player, "startLicenseExam", NAPI.Util.ToJson(questions), NAPI.Util.ToJson(answers));
                                    }
                                    else
                                    {
                                        String message = String.Format(Messages.ERR_DRIVING_SCHOOL_MONEY, Constants.PRICE_DRIVING_THEORICAL);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                                    }
                                    break;
                                case 0:
                                    // Comprobamos si tiene dinero suficiente
                                    if (money >= Constants.PRICE_DRIVING_PRACTICAL)
                                    {
                                        // Añadimos el tipo de licencia y cobramos
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_LICENSE_TYPE, Constants.LICENSE_CAR);
                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - Constants.PRICE_DRIVING_PRACTICAL);

                                        // Tiene el teórico aprobado, le toca el práctico
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, 0);

                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_EXAM, Constants.CAR_DRIVING_PRACTICE);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_ENTER_LICENSE_CAR_VEHICLE);
                                    }
                                    else
                                    {
                                        String message = String.Format(Messages.ERR_DRIVING_SCHOOL_MONEY, Constants.PRICE_DRIVING_PRACTICAL);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                                    }
                                    break;
                                default:
                                    // Tiene puntos en la licencia
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_ALREADY_LICENSE);
                                    break;
                            }
                            break;
                        case "motocicleta":
                            // Miramos el estado de la licencia de motocicletas
                            licenseStatus = GetPlayerLicenseStatus(player, Constants.LICENSE_MOTORCYCLE);
                            switch (licenseStatus)
                            {
                                case -1:
                                    // Comprobamos si tiene dinero suficiente
                                    if (money >= Constants.PRICE_DRIVING_THEORICAL)
                                    {
                                        // No ha aprobado ningún examen
                                        questions = Database.GetRandomQuestions(Constants.LICENSE_MOTORCYCLE + 1);
                                        foreach (TestModel question in questions)
                                        {
                                            answers.AddRange(Database.GetQuestionAnswers(question.id));
                                        }

                                        // Añadimos el tipo de licencia y cobramos
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_LICENSE_TYPE, Constants.LICENSE_MOTORCYCLE);
                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_LICENSE_QUESTION, 0);
                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - Constants.PRICE_DRIVING_THEORICAL);

                                        // Mostramos la pantalla del examen teórico
                                        NAPI.ClientEvent.TriggerClientEvent(player, "startLicenseExam", NAPI.Util.ToJson(questions), NAPI.Util.ToJson(answers));
                                    }
                                    else
                                    {
                                        String message = String.Format(Messages.ERR_DRIVING_SCHOOL_MONEY, Constants.PRICE_DRIVING_THEORICAL);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                                    }
                                    break;
                                case 0:
                                    // Comprobamos si tiene dinero suficiente
                                    if (money >= Constants.PRICE_DRIVING_PRACTICAL)
                                    {
                                        // Añadimos el tipo de licencia y cobramos
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_LICENSE_TYPE, Constants.LICENSE_MOTORCYCLE);
                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - Constants.PRICE_DRIVING_PRACTICAL);

                                        // Tiene el teórico aprobado, le toca el práctico
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_CHECKPOINT, 0);
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_EXAM, Constants.MOTORCYCLE_DRIVING_PRACTICE);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_ENTER_LICENSE_BIKE_VEHICLE);
                                    }
                                    else
                                    {
                                        String message = String.Format(Messages.ERR_DRIVING_SCHOOL_MONEY, Constants.PRICE_DRIVING_PRACTICAL);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                                    }
                                    break;
                                default:
                                    // Tiene puntos en la licencia
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

            // Avisamos de que no está en la autoescuela
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_DRIVING_SCHOOL);
        }

        [Command("licencias")]
        public void LicenciasCommand(Client player)
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
                        // Miramos el estado de la licencia de turismos
                        switch (currentLicenseStatus)
                        {
                            case -1:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Turismo: No disponible");
                                break;
                            case 0:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Turismo: Pendiente del examen práctico");
                                break;
                            default:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Turismo: " + currentLicenseStatus + " puntos");
                                break;
                        }
                        break;
                    case Constants.LICENSE_MOTORCYCLE:
                        // Miramos el estado de la licencia de motocicletas
                        switch (currentLicenseStatus)
                        {
                            case -1:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Motocicleta: No disponible");
                                break;
                            case 0:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Motocicleta: Pendiente del examen práctico");
                                break;
                            default:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Motocicleta: " + currentLicenseStatus + " puntos");
                                break;
                        }
                        break;
                    case Constants.LICENSE_TAXI:
                        // Miramos el estado de la licencia de taxis
                        if (currentLicenseStatus == -1)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Taxi: No disponible");
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Taxi: Vigente");
                        }
                        break;
                }
                currentLicense++;
            }
        }
    }
}