using System.Collections.Generic;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using SFCore.Generics;
using SFCore.Utils;
using UnityEngine;

namespace MoreNotches;

/// <summary>
/// GlobalSettings class.
/// </summary>
public class GlobalSettings
{
    /// <summary>
    /// In addition to the 12 vanilla
    /// </summary>
    public int AdditionalNotchAmount = 50 - 12;

    /// <summary>
    /// In addition to the 6 vanilla.
    /// </summary>
    public int AdditionalOvercharmNotchAmount = 20 - 6;
}

/// <summary>
/// Base class.
/// </summary>
public class MoreNotches : GlobalSettingsMod<GlobalSettings>
{
    private static PlayMakerFSM healthBlueHealthFsm = null;

    public override string GetVersion() => SFCore.Utils.Util.GetVersion(Assembly.GetExecutingAssembly());

    public MoreNotches() : base("More Notches")
    {
    }

    public override void Initialize()
    {
        On.GameCameras.Start += AddMasks;
        DoAddNotches(GameCameras.instance);
        DoChangeOverCharmNotches(GameCameras.instance);
        DoChangeCharmDetailNotches(GameCameras.instance);
    }

    private void AddMasks(On.GameCameras.orig_Start orig, GameCameras self)
    {
        orig(self);
        DoAddNotches(self);
        DoChangeOverCharmNotches(self);
        DoChangeCharmDetailNotches(self);
    }

    private void DoAddNotches(GameCameras self)
    {
        LogFine("!DoAddNotches");
        GameObject notchCost1 = self.gameObject.Find("HudCamera").Find("Inventory").Find("Charms").Find("Equipped Charms").Find("Notches").Find("Charm Cost 1");

        for (int i = 0; i < GlobalSettings.AdditionalNotchAmount; i++)
        {
            // 12 notches already exist with numbers 1 - 12, so we add 13 to i for our custom number
            int num = 13 + i;
            if (notchCost1.transform.parent.gameObject.Find($"Charm Cost {num}") != null)
            {
                // assume that entire below thing has already happened
                continue;
            }

            GameObject notchGo = Object.Instantiate(notchCost1, notchCost1.transform.parent);
            notchGo.name = $"Charm Cost {num}";
            SetNotchPositionAndFsm(notchGo, num);
        }

        LogFine("!DoAddNotches");
    }

    private void SetNotchPositionAndFsm(GameObject ob, int num)
    {
        LogFine("!SetNotchPositionAndFsm");
        PlayMakerFSM notchFsm = ob.LocateMyFSM("charm_cost_indicator");
        FsmVariables notchFsmVars = notchFsm.FsmVariables;
        notchFsmVars.GetFsmInt("Indicator Number").Value = num;

        SetNotchPosition(ob, num);
        LogFine("!SetNotchPositionAndFsm");
    }

    private void SetNotchPosition(GameObject ob, int num)
    {
        LogFine("!SetNotchPosition");
        float xPos = -5.74f + (0.81f * (num - 1));

        ob.transform.localPosition = new Vector3(xPos, -5.04f, -5.63f);
        LogFine("!SetNotchPosition");
    }

    private void DoChangeOverCharmNotches(GameCameras self)
    {
        GameObject overIndicator = self.gameObject.Find("HudCamera").Find("Inventory").Find("Charms").Find("Equipped Charms").Find("Notches").Find("Over Indicator");
        PlayMakerFSM overControlFsm = overIndicator.LocateMyFSM("Over Control");

        GameObject notchCost1 = overIndicator.Find("Over 1");
        for (int i = 0; i < GlobalSettings.AdditionalOvercharmNotchAmount; i++)
        {
            // 6 overcharm notches already exist with numbers 1 - 6, so we add 7 to i for our custom number
            int num = 7 + i;
            if (notchCost1.transform.parent.gameObject.Find($"Over {num}") != null)
            {
                // assume that entire below thing has already happened
                continue;
            }

            GameObject overCharmGo = Object.Instantiate(notchCost1, notchCost1.transform.parent);
            overCharmGo.name = $"Over {num}";

            FindChild findChildAction = new FindChild();
            findChildAction.gameObject = overControlFsm.GetAction<FindChild>("Init", 0).gameObject;
            findChildAction.childName = overCharmGo.name;
            findChildAction.storeResult = overControlFsm.GetGameObjectVariable(overCharmGo.name);
            overControlFsm.InsertAction("Init", findChildAction, num - 1);
            overControlFsm.InsertAction("Display Overcharm", findChildAction, num - 1);

            overControlFsm.Fsm.SaveActions();
            overControlFsm.CopyState($"{num - 1}", $"{num}");
            overControlFsm.Fsm.SaveActions();
            overControlFsm.GetAction<IntCompare>($"{num}", 0).integer2 = num;
            overControlFsm.GetAction<ActivateGameObject>($"{num}", 1).gameObject.GameObject = overCharmGo;
            overControlFsm.ChangeTransition($"{num}", FsmEvent.Finished.Name, overControlFsm.GetTransition($"{num - 1}", FsmEvent.Finished.Name).ToState);
            overControlFsm.ChangeTransition($"{num - 1}", FsmEvent.Finished.Name, $"{num}");

            // set position
            float xPos = -5.74f + (0.81f * (num - 1));
            overCharmGo.transform.localPosition = new Vector3(xPos, -5.04f, -6f);

            overControlFsm.Fsm.SaveActions();
        }
    }

    private void DoChangeCharmDetailNotches(GameCameras self)
    {
        GameObject costIndicator = self.gameObject.Find("HudCamera").Find("Inventory").Find("Charms").Find("Details").Find("Cost");
        PlayMakerFSM costControlFsm = costIndicator.LocateMyFSM("Charm Details Cost");

        GameObject notchCost1 = costIndicator.Find("Cost 1");
        for (int i = 0; i < GlobalSettings.AdditionalOvercharmNotchAmount; i++)
        {
            // 6 overcharm notches already exist with numbers 1 - 6, so we add 7 to i for our custom number
            int num = 7 + i;
            string gameObjectName = $"Cost {num}";
            string prevGameObjectName = $"Cost {num - 1}";
            string fsmFloatVarName = $"{num} X";
            string prevFsmFloatVarName = $"{num - 1} X";
            string fsmGoVarName = $"C{num}";
            if (notchCost1.transform.parent.gameObject.Find(gameObjectName) != null)
            {
                // assume that entire below thing has already happened
                continue;
            }

            GameObject notchCostDetailGo = Object.Instantiate(notchCost1, notchCost1.transform.parent);
            notchCostDetailGo.name = gameObjectName;
            // set position
            float xPos = 12.25f + (0.75f * (num - 1));
            notchCostDetailGo.transform.localPosition = new Vector3(xPos, -3.54f, -5.63f);

            FsmGameObject gameObjectVar = costControlFsm.GetGameObjectVariable(fsmGoVarName);
            FsmFloat positioningVar = costControlFsm.GetFloatVariable(fsmFloatVarName);
            if (num <= 12)
            {
                positioningVar.Value = costControlFsm.GetFloatVariable(prevFsmFloatVarName).Value - 0.375f;
            }
            else
            {
                positioningVar.Value = costControlFsm.GetFloatVariable(prevFsmFloatVarName).Value - 0.75f;
            }

            costControlFsm.InsertAction("Init", new FindChild
            {
                gameObject = new FsmOwnerDefault(costControlFsm.GetAction<FindChild>("Init", num - 2).gameObject),
                childName = gameObjectName,
                storeResult = costControlFsm.GetGameObjectVariable(fsmGoVarName)
            }, num - 1);

            // copy original SetPosition action, save actions, then edit the copied one
            SetPosition origSetPosition = costControlFsm.GetAction<SetPosition>("Cost 0", 0);
            {
                SetPosition copiedSetPosition = new SetPosition();
                copiedSetPosition.gameObject = new FsmOwnerDefault(origSetPosition.gameObject);
                copiedSetPosition.vector = new FsmVector3(origSetPosition.vector);
                copiedSetPosition.x = new FsmFloat(origSetPosition.x);
                copiedSetPosition.y = new FsmFloat(origSetPosition.y);
                copiedSetPosition.z = new FsmFloat(origSetPosition.z);
                copiedSetPosition.space = origSetPosition.space;
                copiedSetPosition.everyFrame = origSetPosition.everyFrame;
                copiedSetPosition.lateUpdate = origSetPosition.lateUpdate;

                costControlFsm.InsertAction("Cost 0", copiedSetPosition, num - 1);
                costControlFsm.Fsm.SaveActions();
                costControlFsm.GetAction<SetPosition>("Cost 0", num - 1).gameObject.GameObject = gameObjectVar;
                costControlFsm.Fsm.SaveActions();
            }
            for (int c = 1; c < num; c++)
            {
                SetPosition copiedSetPosition = new SetPosition();
                copiedSetPosition.gameObject = new FsmOwnerDefault(origSetPosition.gameObject);
                copiedSetPosition.vector = new FsmVector3(origSetPosition.vector);
                copiedSetPosition.x = new FsmFloat(origSetPosition.x);
                copiedSetPosition.y = new FsmFloat(origSetPosition.y);
                copiedSetPosition.z = new FsmFloat(origSetPosition.z);
                copiedSetPosition.space = origSetPosition.space;
                copiedSetPosition.everyFrame = origSetPosition.everyFrame;
                copiedSetPosition.lateUpdate = origSetPosition.lateUpdate;

                // copy original SetPosition action, save actions, then edit the copied one
                costControlFsm.AddAction($"Cost {c}", copiedSetPosition);
                costControlFsm.Fsm.SaveActions();
                costControlFsm.GetAction<SetPosition>($"Cost {c}", num + 3).gameObject.GameObject = gameObjectVar;
                costControlFsm.Fsm.SaveActions();
            }

            // add state and transition
            costControlFsm.Fsm.SaveActions();
            costControlFsm.CopyState(prevGameObjectName, gameObjectName);
            costControlFsm.Fsm.SaveActions();
            FsmEvent fsmEvent = costControlFsm.AddTransition("Check", $"{num}", gameObjectName);
            costControlFsm.Fsm.SaveActions();

            // adjust last state SetPosition to be new fsm float variable
            costControlFsm.GetAction<SetPosition>(gameObjectName, 0).x = positioningVar;
            // adjust last SetPosition to be `Present Y` instead of `Absent Y`
            costControlFsm.GetAction<SetPosition>(gameObjectName, num + 3).y = costControlFsm.FindFloatVariable("Present Y");

            // add option in IntSwitch
            List<FsmInt> adjustedIntList = new List<FsmInt>(costControlFsm.GetAction<IntSwitch>("Check", 1).compareTo);
            adjustedIntList.Add(num);
            List<FsmEvent> adjustedEventList = new List<FsmEvent>(costControlFsm.GetAction<IntSwitch>("Check", 1).sendEvent);
            adjustedEventList.Add(fsmEvent);
            costControlFsm.GetAction<IntSwitch>("Check", 1).compareTo = adjustedIntList.ToArray();
            costControlFsm.GetAction<IntSwitch>("Check", 1).sendEvent = adjustedEventList.ToArray();

            costControlFsm.Fsm.SaveActions();
        }
    }
}