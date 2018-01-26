using GTANetworkAPI;
using WiredPlayers.globals;
using WiredPlayers.model;
using System.Collections.Generic;
using System;

namespace WiredPlayers.faction
{
    public class Job : Script
    {
        private List<JobPickModel> jobList = new List<JobPickModel>()
        {
            new JobPickModel(Constants.JOB_FASTFOOD, new Vector3(-1037.697f, -1397.189f, 5.553192f), Messages.DESC_JOB_FASTFOOT),
            new JobPickModel(Constants.JOB_HOOKER, new Vector3(136.58f, -1278.55f, 29.45f), Messages.DESC_JOB_HOOKER),
            new JobPickModel(Constants.JOB_GARBAGE, new Vector3(-322.088f, -1546.014f, 31.01991f), Messages.DESC_JOB_GARBAGE),
            new JobPickModel(Constants.JOB_MECHANIC, new Vector3(486.5268f, -1314.683f, 29.22961f), Messages.DESC_JOB_MECHANIC),
            new JobPickModel(Constants.JOB_THIEF, new Vector3(-198.225f, -1699.521f, 33.46679f), Messages.DESC_JOB_THIEF) 
        };

        public Job()
        {
            Event.OnResourceStart += OnResourceStart;
        }

        private void OnResourceStart()
        {
            Blip trashBlip = NAPI.Blip.CreateBlip(new Vector3(-322.088f, -1546.014f, 31.01991f));
            NAPI.Blip.SetBlipName(trashBlip, "Trabajo de basurero");
            NAPI.Blip.SetBlipShortRange(trashBlip, true);
            NAPI.Blip.SetBlipSprite(trashBlip, 318);

            Blip mechanicBlip = NAPI.Blip.CreateBlip(new Vector3(486.5268f, -1314.683f, 29.22961f));
            NAPI.Blip.SetBlipName(mechanicBlip, "Trabajo de mecánico");
            NAPI.Blip.SetBlipShortRange(mechanicBlip, true);
            NAPI.Blip.SetBlipSprite(mechanicBlip, 72);

            Blip fastFoodBlip = NAPI.Blip.CreateBlip(new Vector3(-1037.697f, -1397.189f, 5.553192f));
            NAPI.Blip.SetBlipName(fastFoodBlip, "Trabajo de repartidor");
            NAPI.Blip.SetBlipSprite(fastFoodBlip, 501);
            NAPI.Blip.SetBlipShortRange(fastFoodBlip, true);
            
            foreach (JobPickModel job in jobList)
            {
                NAPI.TextLabel.CreateTextLabel("/empleo", job.position, 10.0f, 0.5f, 4, new Color(255, 255, 153), false, 0);
                NAPI.TextLabel.CreateTextLabel("Escribe el comando para obtener más información del empleo", new Vector3(job.position.X, job.position.Y, job.position.Z - 0.1f), 10.0f, 0.5f, 4, new Color(0, 0, 0), false, 0);
            }
        }

        public static int GetJobPoints(Client player, int job)
        {
            String jobPointsString = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_POINTS);
            return Int32.Parse(jobPointsString.Split(',')[job]);
        }

        public static void SetJobPoints(Client player, int job, int points)
        {
            String jobPointsString = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_POINTS);
            String[] jobPointsArray = jobPointsString.Split(',');
            jobPointsArray[job] = points.ToString();
            jobPointsString = String.Join(",", jobPointsArray);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_POINTS, jobPointsString);
        }

        [Command("empleo", Messages.GEN_JOB_COMMAND)]
        public void EmpleoCommand(Client player, String action)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
            int job = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB);

            switch (action.ToLower())
            {
                case "info":
                    foreach (JobPickModel jobPick in jobList)
                    {
                        if (player.Position.DistanceTo(jobPick.position) < 1.5f)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + jobPick.description);
                            break;
                        }
                    }
                    break;
                case "aceptar":
                    if (faction > 0 && faction < Constants.LAST_STATE_FACTION)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_JOB_STATE_FACTION);
                    }
                    else if (job > 0)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_HAS_JOB);
                    }
                    else
                    {
                        foreach (JobPickModel jobPick in jobList)
                        {
                            if (player.Position.DistanceTo(jobPick.position) < 1.5f)
                            {
                                NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB, jobPick.job);
                                NAPI.Data.SetEntityData(player, EntityData.PLAYER_EMPLOYEE_COOLDOWN, 5);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_JOB_ACCEPTED);
                                break;
                            }
                        }
                    }
                    break;
                case "dejar":
                    // Obtenemos el tiempo entre trabajo y trabajo
                    int employeeCooldown = NAPI.Data.GetEntityData(player, EntityData.PLAYER_EMPLOYEE_COOLDOWN);

                    if (employeeCooldown > 0)
                    {
                        String message = String.Format(Messages.ERR_EMPLOYEE_COOLDOWN, employeeCooldown);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                    }
                    else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_RESTRICTION) > 0)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_JOB_RESTRICTION);
                    }
                    else if (job == 0)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_JOB);
                    }
                    else
                    {
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB, 0);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_JOB_LEFT);
                    }
                    break;
                default:
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_JOB_COMMAND);
                    break;
            }
        }

        [Command("deservicio")]
        public void DeservicioCommand(Client player)
        {
            // Obtenemos el sexo, trabajo y la facción
            int playerSex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
            int playerJob = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB);
            int playerFaction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);

            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (playerJob == 0 && playerFaction == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_JOB);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 1)
            {
                // Generación de la ropa del personaje
                Globals.PopulateCharacterClothes(player);

                // Quitamos el estado de servicio
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_ON_DUTY, 0);

                // Notificación al jugador
                NAPI.Notification.SendNotificationToPlayer(player, Messages.INF_PLAYER_FREE_TIME);
            }
            else
            {
                // Añadimos toda la ropa disponible
                foreach (UniformModel uniform in Constants.UNIFORM_LIST)
                {
                    if (uniform.type == 0 && uniform.factionJob == playerFaction && playerSex == uniform.characterSex)
                    {
                        NAPI.Player.SetPlayerClothes(player, uniform.uniformSlot, uniform.uniformDrawable, uniform.uniformTexture);
                    }
                    else if (uniform.type == 1 && uniform.factionJob == playerJob && playerSex == uniform.characterSex)
                    {
                        NAPI.Player.SetPlayerClothes(player, uniform.uniformSlot, uniform.uniformDrawable, uniform.uniformTexture);
                    }
                }

                // Marcamos al jugador de servicio
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_ON_DUTY, 1);

                // Notificación al jugador
                NAPI.Notification.SendNotificationToPlayer(player, Messages.INF_PLAYER_ON_DUTY);
            }
        }
    }
}
