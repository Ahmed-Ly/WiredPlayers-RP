using GTANetworkAPI;
using WiredPlayers.globals;
using System;

namespace WiredPlayers.Animations
{
    public class Animations : Script
    {
        [Command(Messages.COM_GRAB)]
        public void GrabCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "anim@heists@money_grab@duffel", "loop");
            }
        }

        [Command(Messages.COM_FACEPALM)]
        public void FacepalmCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intcelebrationmale@face_palm", "face_palm");
            }
        }

        [Command(Messages.COM_LOCO)]
        public void LocoCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intcelebrationmale@you_loco", "you_loco");
            }
        }

        [Command(Messages.COM_FREAKOUT)]
        public void FreakoutCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intcelebrationmale@freakout", "freakout");
            }
        }

        [Command(Messages.COM_THUMB_ON_EARS)]
        public void ThumbOnEarsCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intcelebrationmale@thumb_on_ears", "thumb_on_ears");
            }
        }

        [Command(Messages.COM_VICTORY)]
        public void VictoryCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intcelebrationmale@v_sign", "v_sign");
            }
        }

        [Command(Messages.COM_CROUCH)]
        public void CrouchCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "misscarstealfinalecar_5_ig_3", "crouchloop");
            }
        }

        [Command(Messages.COM_DJ)]
        public void DjCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intupperdj", "enter");
            }
        }

        [Command(Messages.COM_KNEEL)]
        public void KneelCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@medic@standing@kneel@base", "base");
            }
        }

        [Command(Messages.COM_SPEAK)]
        public void SpeakCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@code_human_police_crowd_control@idle_a", "idle_b");
            }
        }

        [Command(Messages.COM_MECHANIC, Messages.GEN_ANIMS_MECHANIC)]
        public void MechanicCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_vehicle_mechanic@male@idle_a", "idle_a");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@repair", "fixing_a_ped");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missheistdockssetup1ig_10@laugh", "laugh_pipe_worker1");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_MECHANIC);
                        break;
                }
            }
        }

        [Command(Messages.COM_DIG)]
        public void DigCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missmic1leadinoutmic_1_mcs_2", "_leadin_trevor");
            }
        }

        [Command(Messages.COM_CRY)]
        public void CryCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mp_bank_heist_1", "f_cower_01");
            }
        }

        [Command(Messages.COM_CLEAN, Messages.GEN_ANIMS_CLEAN)]
        public void CleanCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "switch@franklin@cleaning_car", "001946_01_gc_fras_v2_ig_5_base");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "timetable@maid@cleaning_window@base", "base");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missheistdocks2bleadinoutlsdh_2b_int", "leg_massage_b_floyd");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missfbi_s4mop", "idle_scrub");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_bum_wash@male@low@idle_a", "idle_c");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_CLEAN);
                        break;
                }
            }
        }

        [Command(Messages.COM_SHOWER, Messages.GEN_ANIMS_SHOWER)]
        public void ShowerCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "anim@mp_yacht@shower@male@", "male_shower_idle_d");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "anim@mp_yacht@shower@female@", "shower_idle_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_SHOWER);
                        break;
                }
            }
        }

        [Command(Messages.COM_HURRY_UP)]
        public void HurryUpCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missfam4", "say_hurry_up_a_trevor");
            }
        }

        [Command(Messages.COM_SPORTS, Messages.GEN_ANIMS_SPORTS)]
        public void SportsCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "rcmcollect_paperleadinout@", "meditiate_idle");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "timetable@reunited@ig_2", "jimmy_masterbation");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "timetable@reunited@ig_2", "jimmy_base");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@jog@", "run");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_f@jogger", "jogging");
                        break;
                    case 6:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "amb@world_human_jog@female@base", "base");
                        break;
                    case 7:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@triathlon", "idle_a");
                        break;
                    case 8:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@triathlon", "idle_d");
                        break;
                    case 9:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@triathlon", "idle_e");
                        break;
                    case 10:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@triathlon", "idle_f");
                        break;
                    case 11:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_yoga@female@base", "base_a");
                        break;
                    case 12:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_yoga@female@base", "base_c");
                        break;
                    case 13:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_yoga@male@base", "base_b");
                        break;
                    case 14:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_push_ups@male@idle_a", "idle_d");
                        break;
                    case 15:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_sit_ups@male@base", "base");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_SPORTS);
                        break;
                }
            }
        }

        [Command(Messages.COM_TYPE)]
        public void TypeCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mp_fbi_heist", "loop");
            }
        }

        [Command(Messages.COM_KNOCK_DOOR)]
        public void KnockDoorCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "timetable@jimmy@doorknock@", "knockdoor_idle");
            }
        }

        [Command(Messages.COM_TAGGING)]
        public void TaggingCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "switch@franklin@lamar_tagging_wall", "lamar_tagging_exit_loop_lamar");
            }
        }

        [Command(Messages.COM_STRIPTEASE, Messages.GEN_ANIMS_STRIPTEASE)]
        public void StripteaseCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.StopOnLastFrame), "mini@strip_club@pole_dance@pole_a_2_stage", "pole_a_2_stage");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.StopOnLastFrame), "mini@strip_club@pole_dance@pole_b_2_stage", "pole_b_2_stage");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.StopOnLastFrame), "mini@strip_club@pole_dance@pole_c_2_prvd_a", "pole_c_2_prvd_a");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.StopOnLastFrame), "mini@strip_club@pole_dance@pole_c_2_prvd_b", "pole_c_2_prvd_b");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@hookers_spcrackhead", "idle_b");
                        break;
                    case 6:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.StopOnLastFrame), "mini@strip_club@pole_dance@pole_c_2_prvd_c", "pole_c_2_prvd_c");
                        break;
                    case 7:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@strip_club@pole_dance@pole_dance1", "pd_dance_01");
                        break;
                    case 8:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@strip_club@pole_dance@pole_dance2", "pd_dance_02");
                        break;
                    case 9:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@strip_club@pole_dance@pole_dance3", "pd_dance_03");
                        break;
                    case 10:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@strip_club@pole_dance@pole_enter", "pd_enter");
                        break;
                    case 11:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@strip_club@pole_dance@pole_exit", "pd_exit");
                        break;
                    case 12:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@strip_club@private_dance@exit", "priv_dance_exit");
                        break;
                    case 13:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@strip_club@private_dance@idle", "priv_dance_idle");
                        break;
                    case 14:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mp_am_stripper", "lap_dance_girl");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_STRIPTEASE);
                        break;
                }
            }
        }

        [Command(Messages.COM_DRINK)]
        public void DrinkCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "amb@world_human_drinking_fat@beer@male@idle_a", "idle_a");
            }
        }

        [Command(Messages.COM_KISS)]
        public void KissCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "mini@hookers_sp", "idle_a");
            }
        }

        [Command(Messages.COM_AIM)]
        public void AimCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "combat@chg_stance", "aima_loop");
            }
        }

        [Command(Messages.COM_SALUTE, Messages.GEN_ANIMS_SALUTE)]
        public void SaluteCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mp_player_int_uppersalute", "mp_player_int_salute");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, 0, "mp_ped_interaction", "hugs_guy_a");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, 0, "mp_player_intsalute", "mp_player_int_salute");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missmic4premiere", "crowd_a_idle_01");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missexile2", "franklinwavetohelicopter");
                        break;
                    case 6:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intcelebrationmale@wave", "wave");
                        break;
                    default:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_SALUTE);
                        break;
                }
            }
        }

        [Command(Messages.COM_FUCKU)]
        public void FuckUCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "anim@mp_player_intselfiethe_bird", "idle_a");
            }
        }

        [Command(Messages.COM_WALK, Messages.GEN_ANIMS_WALK)]
        public void WalkCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_f@heels@c", "walk");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_f@arrogant@a", "walk");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_f@sad@a", "walk");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@drunk@moderatedrunk", "walk");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@shadyped@a", "walk");
                        break;
                    case 6:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_f@gangster@ng", "walk");
                        break;
                    case 7:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_f@generic", "walk");
                        break;
                    case 8:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_f@heels@d", "walk");
                        break;
                    case 9:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_f@posh@", "walk");
                        break;
                    case 10:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@brave@b", "walk");
                        break;
                    case 11:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@confident", "walk");
                        break;
                    case 12:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@depressed@d", "walk");
                        break;
                    case 13:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@favor_right_foot", "walk");
                        break;
                    case 14:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@generic", "walk");
                        break;
                    case 15:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@generic_variations@walk", "walk_a");
                        break;
                    case 16:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@generic_variations@walk", "walk_f");
                        break;
                    case 17:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@golfer@", "golf_walk");
                        break;
                    case 18:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@money", "walk");
                        break;
                    case 19:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@shadyped@a", "walk");
                        break;
                    case 20:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "move_m@swagger@b", "walk");
                        break;
                    case 21:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "switch@franklin@dispensary", "exit_dispensary_outro_ped_f_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_WALK);
                        break;
                }
            }
        }

        [Command(Messages.COM_KNUCKLES)]
        public void KnucklesCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "anim@mp_player_intupperknuckle_crunch", "idle_a");
            }
        }

        [Command(Messages.COM_SURRENDER, Messages.GEN_ANIMS_SURRENDER)]
        public void SurrenderCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "mp_am_hold_up", "handsup_base");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "anim@mp_player_intuppersurrender", "idle_a_fp");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "amb@code_human_cower@female@react_cowering", "base_back_left");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "amb@code_human_cower@female@react_cowering", "base_right");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missfbi5ig_0", "lyinginpain_loop_steve");
                        break;
                    case 6:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missfbi5ig_10", "lift_holdup_loop_labped");
                        break;
                    case 7:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missfbi5ig_17", "walk_in_aim_loop_scientista");
                        break;
                    case 8:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mp_am_hold_up", "cower_loop");
                        break;
                    case 9:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mp_arrest_paired", "crook_p1_idle");
                        break;
                    case 10:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mp_bank_heist_1", "m_cower_02");
                        break;
                    case 11:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "misstrevor1", "threaten_ortega_endloop_ort");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_SURRENDER);
                        break;
                }
            }
        }

        [Command(Messages.COM_EAT)]
        public void EatCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "mp_player_inteat@burger", "mp_player_int_eat_burger");
            }
        }

        [Command(Messages.COM_PUKE)]
        public void PukeCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "missheistpaletoscore1leadinout", "trv_puking_leadout");
            }
        }

        [Command(Messages.COM_PLANT)]
        public void PlantCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "amb@world_human_gardener_plant@male@idle_a", "idle_a");
            }
        }

        [Command(Messages.COM_CPR, Messages.GEN_ANIMS_CPR)]
        public void CprCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.StopOnLastFrame | Constants.AnimationFlags.AllowPlayerControl), "mini@cpr@char_a@cpr_def", "cpr_intro");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "mini@cpr@char_a@cpr_str", "cpr_kol");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "mini@cpr@char_a@cpr_def", "cpr_pumpchest_idle");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, 0, "mini@cpr@char_a@cpr_str", "cpr_success");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_CPR);
                        break;
                }
            }
        }

        [Command(Messages.COM_CAR_SEX, Messages.GEN_ANIMS_CAR_SEX)]
        public void CarSexCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Player.IsPlayerInAnyVehicle(player) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE);
            }
            else
            {
                NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);
                String vehicleModel = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_MODEL);
                VehicleHash vehicleHash = NAPI.Util.VehicleNameToModel(vehicleModel);
                if (NAPI.Vehicle.GetVehicleClass(vehicleHash) == Constants.VEHICLE_CLASS_CYCLES || NAPI.Vehicle.GetVehicleClass(vehicleHash) == Constants.VEHICLE_CLASS_MOTORCYCLES || NAPI.Vehicle.GetVehicleClass(vehicleHash) == Constants.VEHICLE_CLASS_BOATS)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_CAR);
                }
                else
                {
                    switch (action)
                    {
                        case 1:
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != (int)VehicleSeat.Driver)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_DRIVING);
                            }
                            else
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@prostitutes@sexnorm_veh", "bj_loop_male");
                            }
                            break;
                        case 2:
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != (int)VehicleSeat.Driver)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_DRIVING);
                            }
                            else
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@prostitutes@sexnorm_veh", "sex_loop_male");
                            }
                            break;
                        case 3:
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != (int)VehicleSeat.RightFront)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_PASSENGER);
                            }
                            else
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@prostitutes@sexnorm_veh", "sex_loop_prostitute");
                            }
                            break;
                        case 4:
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != (int)VehicleSeat.Driver)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_DRIVING);
                            }
                            else
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@prostitutes@sexlow_veh", "low_car_bj_loop_player");
                            }
                            break;
                        case 5:
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != (int)VehicleSeat.RightFront)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_PASSENGER);
                            }
                            else
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@prostitutes@sexlow_veh", "low_car_bj_loop_female");
                            }
                            break;
                        case 6:
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != (int)VehicleSeat.RightFront)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_PASSENGER);
                            }
                            else
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@prostitutes@sexlow_veh", "low_car_sex_loop_female");
                            }
                            break;
                        default:
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_CAR_SEX);
                            break;
                    }
                }
            }
        }

        [Command(Messages.COM_SEXY_DANCE, Messages.GEN_ANIMS_SEXY_DANCE)]
        public void SexyDanceCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_prostitute@cokehead@idle_a", "idle_b");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, 0, "mini@hookers_sp", "ilde_c");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, 0, "mini@hookers_spcokehead", "idle_a");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, 0, "mini@hookers_spcokehead", "idle_c");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@hookers_spcrackhead", "idle_b");
                        break;
                    case 6:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, 0, "mini@hookers_spvanilla", "idle_b");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_SEXY_DANCE);
                        break;
                }
            }
        }

        [Command(Messages.COM_SIT, Messages.GEN_ANIMS_SIT)]
        public void SitCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_stupor@male@base", "base");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_stupor@male_looking_left@base", "base");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "anim@heists@fleeca_bank@ig_7_jetski_owner", "owner_idle");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mp_army_contact", "positive_a");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "timetable@reunited@ig_10", "base_amanda");
                        break;
                    case 6:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "anim@heists@prison_heistunfinished_biztarget_idle", "target_idle");
                        break;
                    case 7:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "switch@michael@sitting", "idle");
                        break;
                    case 8:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "timetable@michael@on_sofaidle_c", "sit_sofa_g");
                        break;
                    case 9:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "timetable@michael@on_sofaidle_b", "sit_sofa_d");
                        break;
                    case 10:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "timetable@michael@on_sofaidle_a", "sit_sofa_a");
                        break;
                    case 11:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "rcm_barry3", "barry_3_sit_loop");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_SIT);
                        break;
                }
            }
        }

        [Command(Messages.COM_SMOKE, Messages.GEN_ANIMS_SMOKING)]
        public void SmokeCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_smoking@male@male_a@idle_a", "idle_c");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.OnlyAnimateUpperBody | Constants.AnimationFlags.AllowPlayerControl), "amb@world_human_smoking@female@idle_a", "idle_b");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "mini@hookers_spfrench", "idle_wait");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_SMOKING);
                        break;
                }
            }
        }

        [Command(Messages.COM_LIE_DOWN, Messages.GEN_ANIMS_LIE_DOWN)]
        public void LieDownCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_sunbathe@male@back@idle_a", "idle_a");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_sunbathe@female@back@idle_a", "idle_a");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_sunbathe@female@front@base", "base");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_picnic@male@base", "base");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_picnic@female@base", "base");
                        break;
                    case 6:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missfra0_chop_fchase", "ballasog_rollthroughtraincar_ig6_loop");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_LIE_DOWN);
                        break;
                }
            }
        }

        [Command(Messages.COM_ARMS, Messages.GEN_ANIMS_ARMS)]
        public void ArmsCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_hang_out_street@male_c@base", "base");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_hang_out_street@female_arms_crossed@idle_a", "idle_a");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "mini@hookers_sp", "idle_reject_loop_c");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "mini@hookers_sp", "idle_reject");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_ARMS);
                        break;
                }
            }
        }

        [Command(Messages.COM_GUARD, Messages.GEN_ANIMS_GUARD)]
        public void GuardCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missbigscore1", "idle_a");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missbigscore1", "idle_base");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missbigscore1", "idle_c");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missbigscore1", "idle_e");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_GUARD);
                        break;
                }
            }
        }

        [Command(Messages.COM_DEAD, Messages.GEN_ANIMS_DEAD)]
        public void DeadCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missarmenian2", "corpse_search_exit_ped");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missarmenian2", "drunk_loop");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missfinale_c1@", "lying_dead_player0");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mp_bank_heist_1", "prone_l_loop");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missfra2", "lamar_base_idle");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_DEAD);
                        break;
                }
            }
        }

        [Command(Messages.COM_IDLE, Messages.GEN_ANIMS_IDLE)]
        public void IdleCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.OnlyAnimateUpperBody | Constants.AnimationFlags.AllowPlayerControl), "amb@world_human_hang_out_street@female_hold_arm@base", "base");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "mini@hookers_sp", "idle_wait");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_stand_impatient@female@no_sign@base", "base");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@hookers_spfrench", "idle_wait");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_hang_out_street@female_arm_side@idle_a", "idle_a");
                        break;
                    case 6:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_muscle_flex@arms_in_front@idle_a", "idle_b");
                        break;
                    case 7:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missfbi5leadinout", "leadin_2_fra");
                        break;
                    case 8:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_cop_idles@female@idle_a", "idle_d");
                        break;
                    case 9:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_cop_idles@male@idle_b", "idle_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_IDLE);
                        break;
                }
            }
        }

        [Command(Messages.COM_PHONE, Messages.GEN_ANIMS_TLF)]
        public void PhoneCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "amb@world_human_stand_mobile@male@text@base", "base");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "cellphone@", "cellphone_email_read_base");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "cellphone@", "cellphone_photo_idle");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.OnlyAnimateUpperBody | Constants.AnimationFlags.AllowPlayerControl), "amb@world_human_stand_mobile@female@standing@call@idle_a", "idle_a");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_mobile_film_shocking@female@idle_a", "idle_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_TLF);
                        break;
                }
            }
        }

        [Command(Messages.COM_LEAN, Messages.GEN_ANIMS_LEAN)]
        public void LeanCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@prop_human_bum_shopping_cart@male@idle_a", "idle_a");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "anim@mp_ferris_wheel", "idle_a_player_one");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@prop_human_bum_shopping_cart@male@base", "base");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@legs_crossed@idle_b", "idle_d");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@hands_together@idle_a", "idle_c");
                        break;
                    case 6:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@mobile@base", "base");
                        break;
                    case 7:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@texting@base", "base");
                        break;
                    case 8:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@mobile@base", "base");
                        break;
                    case 9:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@texting@base", "base");
                        break;
                    case 10:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_leaning@female@smoke@idle_a", "idle_a");
                        break;
                    case 11:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@foot_up@idle_b", "idle_d");
                        break;
                    case 12:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "misscarsteal1car_1_ext_leadin", "base_driver1");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_LEAN);
                        break;
                }
            }
        }

        [Command(Messages.COM_CHEER, Messages.GEN_ANIMS_CHEER)]
        public void CheerCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_cheering@female_a", "base");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_cheering@female_c", "base");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_cheering@female_d", "base");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_cheering@male_a", "base");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_cheering@male_b", "base");
                        break;
                    case 6:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_cheering@male_d", "base");
                        break;
                    case 7:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_cheering@male_e", "base");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_CHEER);
                        break;
                }
            }
        }

        [Command(Messages.COM_DANCE, Messages.GEN_ANIMS_DANCE)]
        public void DanceCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_jog_standing@female@base", "base");
                        break;
                    case 2:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_jog_standing@female@idle_a", "idle_a");
                        break;
                    case 3:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_power_walker@female@static", "static");
                        break;
                    case 4:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_partying@female@partying_beer@base", "base");
                        break;
                    case 5:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_partying@female@partying_cellphone@idle_a", "idle_a");
                        break;
                    case 6:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_partying@female@partying_beer@idle_a", "idle_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_DANCE);
                        break;
                }
            }
        }

        [Command(Messages.COM_PISS)]
        public void PissCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missbigscore1switch_trevor_piss", "piss_loop");
            }
        }

        [Command(Messages.COM_APLAUSE)]
        public void AplauseCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_strip_watch_stand@male_a@idle_a", "idle_a");
            }
        }

        [Command(Messages.COM_DRUNK)]
        public void DrunkCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "mini@hookers_spcrackhead", "idle_c");
            }
        }

        [Command(Messages.COM_SHRUG)]
        public void ShrugCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "gestures@f@standing@casual", "gesture_shrug_hard");
            }
        }

        [Command(Messages.COM_DESPERATE)]
        public void DesperateCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "misscarsteal4@toilet", "desperate_toilet_idle_a");
            }
        }

        [Command(Messages.COM_PENSIVE)]
        public void PensiveCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.OnlyAnimateUpperBody | Constants.AnimationFlags.AllowPlayerControl), "misscarsteal4@aliens", "rehearsal_base_idle_director");
            }
        }

        [Command(Messages.COM_HANDS_HEAT)]
        public void HandsHeatCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_stand_fire@male@base", "base");
            }
        }

        [Command(Messages.COM_ROCK)]
        public void RockCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "anim@mp_player_intincarrockbodhi@ps@", "enter");
            }
        }

        [Command(Messages.COM_INJURED)]
        public void InjuredCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.StopOnLastFrame | Constants.AnimationFlags.AllowPlayerControl), "combat@damage@injured_pistol@to_writhe", "variation_b");
            }
        }

        [Command(Messages.COM_STUMBLE)]
        public void StumbleCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "misscarsteal4@actor", "stumble");
            }
        }
    }

}
