using GTANetworkAPI;
using WiredPlayers.globals;
using System;

namespace WiredPlayers.Animations
{
    public class Animations : Script
    {
        public Animations()
        {
        }

        [Command("recogiendo")]
        public void recogiendoComamnd(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "anim@heists@money_grab@duffel", "loop");
            }
        }

        [Command("facepalm")]
        public void facepalmComamnd(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intcelebrationmale@face_palm", "face_palm");
            }
        }

        [Command("loco")]
        public void locoComamnd(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intcelebrationmale@you_loco", "you_loco");
            }
        }

        [Command("flipar")]
        public void fliparComamnd(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intcelebrationmale@freakout", "freakout");
            }
        }

        [Command("burla")]
        public void tauntComamnd(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intcelebrationmale@thumb_on_ears", "thumb_on_ears");
            }
        }

        [Command("paz")]
        public void pazComamnd(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intcelebrationmale@v_sign", "v_sign");
            }
        }

        [Command("esconderse")]
        public void esconderseComamnd(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "misscarstealfinalecar_5_ig_3", "crouchloop");
            }
        }

        [Command("dj")]
        public void djComamnd(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "anim@mp_player_intupperdj", "enter");
            }
        }

        [Command("arrodillarse")]
        public void arrodillarseCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@medic@standing@kneel@base", "base");
            }
        }

        [Command("hablar")]
        public void hablarCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@code_human_police_crowd_control@idle_a", "idle_b");
            }
        }

        [Command("mecanico", Messages.GEN_ANIMS_MECHANIC)] 
        public void mecanicoCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("cavar")] 
        public void cavarCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missmic1leadinoutmic_1_mcs_2", "_leadin_trevor");
            }
        }

        [Command("llorar")]
        public void llorarsueloCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mp_bank_heist_1", "f_cower_01");
            }
        }

        [Command("limpiar", Messages.GEN_ANIMS_CLEAN)]
        public void limpiarCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("ducharse", Messages.GEN_ANIMS_SHOWER)]
        public void ducharCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("venaqui")] 
        public void vamosCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missfam4", "say_hurry_up_a_trevor");
            }
        }

        [Command("deporte", Messages.GEN_ANIMS_SPORTS)]
        public void deporteCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("teclear")]
        public void ordenadorCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mp_fbi_heist", "loop");
            }
        }

        [Command("llamarpuerta")]
        public void llamarpuertaCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "timetable@jimmy@doorknock@", "knockdoor_idle");
            }
        }

        [Command("graffiti")]
        public void graffitiCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "switch@franklin@lamar_tagging_wall", "lamar_tagging_exit_loop_lamar");
            }
        }

        [Command("striptease", Messages.GEN_ANIMS_STRIPTEASE)]
        public void stripteaseCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("beber")]
        public void beberCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "amb@world_human_drinking_fat@beer@male@idle_a", "idle_a");
            }
        }

        [Command("beso", Messages.GEN_ANIMS_KISS)]
        public void besoCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "mini@hookers_sp", "idle_a");
            }
        }

        [Command("apuntar", Messages.GEN_ANIMS_AIM)]
        public void apuntarCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                switch (action)
                {
                    case 1:
                        NAPI.Player.StopPlayerAnimation(player);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "combat@chg_stance", "aima_loop");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_AIM);
                        break;
                }
            }
        }

        [Command("saludar", Messages.GEN_ANIMS_SALUTE)]
        public void saludoCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("cortemangas")]
        public void cortemangasCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "anim@mp_player_intselfiethe_bird", "idle_a");
            }
        }

        [Command("andar", Messages.GEN_ANIMS_WALK)]
        public void caminarCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("nudillos")]
        public void nudillosCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "anim@mp_player_intupperknuckle_crunch", "idle_a");
            }
        }

        [Command("rendirse", Messages.GEN_ANIMS_SURRENDER)]
        public void rendirseCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("comer")]
        public void comerCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "mp_player_inteat@burger", "mp_player_int_eat_burger");
            }
        }

        [Command("vomitar")]
        public void vomitarCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "missheistpaletoscore1leadinout", "trv_puking_leadout");
            }
        }

        [Command("plantar")]
        public void plantarCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "amb@world_human_gardener_plant@male@idle_a", "idle_a");
            }
        }

        [Command("rcp", Messages.GEN_ANIMS_PCR)]
        public void rcpCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_ANIMS_PCR);
                        break;
                }
            }
        }

        [Command("sexocoche", Messages.GEN_ANIMS_CAR_SEX)]
        public void sexococheCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != Constants.VEHICLE_SEAT_DRIVER)
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
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != Constants.VEHICLE_SEAT_DRIVER)
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
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != Constants.VEHICLE_SEAT_PASSENGER)
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
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != Constants.VEHICLE_SEAT_DRIVER)
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
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != Constants.VEHICLE_SEAT_PASSENGER)
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
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != Constants.VEHICLE_SEAT_PASSENGER)
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
        
        [Command("bailesexy", Messages.GEN_ANIMS_SEXY_DANCE)]
        public void bailarsexyCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("sentarse", Messages.GEN_ANIMS_SIT)]
        public void sentarseCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("fumar", Messages.GEN_ANIMS_SMOKING)]
        public void fumarCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("tumbarse", Messages.GEN_ANIMS_LIE_DOWN)]
        public void tumbarseCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("brazos", Messages.GEN_ANIMS_ARMS)]
        public void brazosCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("guardia", Messages.GEN_ANIMS_GUARD)]
        public void guardiaCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("muerto", Messages.GEN_ANIMS_DEAD)]
        public void muertoCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("pose", Messages.GEN_ANIMS_IDLE)]
        public void poseCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("telefono", Messages.GEN_ANIMS_TLF)]
        public void telefonoCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("apoyarse", Messages.GEN_ANIMS_LEAN)]
        public void apoyarsebarraCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("animar", Messages.GEN_ANIMS_CHEER)]
        public void animarfemCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("bailar", Messages.GEN_ANIMS_DANCE)]
        public void bailarCommand(Client player, int action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
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

        [Command("mear")]
        public void mearCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missbigscore1switch_trevor_piss", "piss_loop");
            }
        }

        [Command("aplaudir")]
        public void aplaudirM1Command(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_strip_watch_stand@male_a@idle_a", "idle_a");
            }
        }

        [Command("borracha")]
        public void borrachaCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "mini@hookers_spcrackhead", "idle_c");
            }
        }

        [Command("indiferencia")] 
        public void indiferenciaCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "gestures@f@standing@casual", "gesture_shrug_hard");
            }
        }

        [Command("nervios")]
        public void nerviosCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "misscarsteal4@toilet", "desperate_toilet_idle_a");
            }
        }

        [Command("pensativo")]
        public void pensativoCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.OnlyAnimateUpperBody | Constants.AnimationFlags.AllowPlayerControl), "misscarsteal4@aliens", "rehearsal_base_idle_director");
            }
        }

        [Command("calentarmanos")]
        public void calentarmanosCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "amb@world_human_stand_fire@male@base", "base");
            }
        }

        [Command("rock")]
        public void rockCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "anim@mp_player_intincarrockbodhi@ps@", "enter");
            }
        }

        [Command("contoneo")]
        public void provocarsexCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "mini@hookers_spcokehead", "idle_a");
            }
        }

        [Command("malherido")]
        public void heridoCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.StopOnLastFrame | Constants.AnimationFlags.AllowPlayerControl), "combat@damage@injured_pistol@to_writhe", "variation_b");
            }
        }

        [Command("tefo")]
        public void tefoCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl | Constants.AnimationFlags.OnlyAnimateUpperBody), "anim@mp_player_intupperdock", "idle_a");
            }
        }

        [Command("golpeado")]
        public void golpeadoCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEATH);
            }
            else
            {
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.PlayPlayerAnimation(player, 0, "misscarsteal4@actor", "stumble");
            }
        }
    }
    
}
