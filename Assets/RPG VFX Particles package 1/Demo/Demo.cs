//====================Copyright statement:AppsTools===================//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Demo : MonoBehaviour
{
    string ss = "Prefabs/Dark/Effect_Skill_Dark_01_Hit.prefab&Prefabs/Dark/Effect_Skill_Dark_01_Start.prefab&Prefabs/DustSmoke/Effect_Skill_DustSmoke_01_Start.prefab&Prefabs/Fire/Effect_Skill_Fire_01.prefab&Prefabs/Fire/Effect_Skill_Fire_02_Hit.prefab&Prefabs/Fire/Effect_Skill_Fire_02_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_01_Hit.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_01_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_02_Hit.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_02_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_03.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_04_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_05_Hit.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_05_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_06_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_07_Hit.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_07_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_08_Hit.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_08_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_09_Hit.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_09_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_10_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_11_Hit1.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_11_Hit2.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_11_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_12_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_13_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_14_Loop.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_14_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_15_Loop.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_15_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_16_17_Hit.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_16_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_17_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_18_Hit1.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_18_Hit2.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_18_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_19_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_20_Hit.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_20_Start.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_21_Hit.prefab&Prefabs/Fire/Attack/Effect_Skill_Attack_21_Start.prefab&Prefabs/Fire/Fireball/Effect_Skill_Fireball_01_Fly.prefab&Prefabs/Fire/Fireball/Effect_Skill_Fireball_01_Hit.prefab&Prefabs/Fire/Fireball/Effect_Skill_Fireball_01_Start.prefab&Prefabs/Fire/Fireball/Effect_Skill_Fireball_02_Fly.prefab&Prefabs/Golden/Effect_Skill_Golden_01_Hit.prefab&Prefabs/Golden/Effect_Skill_Golden_01_Start.prefab&Prefabs/Golden/Effect_Skill_Golden_02_Start.prefab&Prefabs/Golden/Effect_Skill_Golden_03_Hit.prefab&Prefabs/Golden/Effect_Skill_Golden_03_Start.prefab&Prefabs/Golden/Effect_Skill_Golden_04_Hit.prefab&Prefabs/Golden/Effect_Skill_Golden_04_Start.prefab&Prefabs/Golden/Effect_Skill_Golden_05_Hit1.prefab&Prefabs/Golden/Effect_Skill_Golden_05_Hit2.prefab&Prefabs/Golden/Effect_Skill_Golden_05_Start.prefab&Prefabs/Golden/Effect_Skill_Golden_06_Hit.prefab&Prefabs/Golden/Effect_Skill_Golden_06_Hit2.prefab&Prefabs/Golden/Effect_Skill_Golden_06_Start.prefab&Prefabs/Golden/Effect_Skill_Golden_07_Fly.prefab&Prefabs/Green/Effect_Skill_Green_01_Fly.prefab&Prefabs/Green/Effect_Skill_Green_01_Hit.prefab&Prefabs/Green/Effect_Skill_Green_01_Start.prefab&Prefabs/Green/Effect_Skill_Green_02_Fly.prefab&Prefabs/Green/Effect_Skill_Green_02_Hit1.prefab&Prefabs/Green/Effect_Skill_Green_02_Hit2.prefab&Prefabs/Green/Effect_Skill_Green_02_Start.prefab&Prefabs/Green/Effect_Skill_Green_03_Hit1.prefab&Prefabs/Green/Effect_Skill_Green_03_Start.prefab&Prefabs/Green/Effect_Skill_Green_04_Fly.prefab&Prefabs/Green/Effect_Skill_Green_04_Hit.prefab&Prefabs/Green/Effect_Skill_Green_04_Loop.prefab&Prefabs/Green/Effect_Skill_Green_05_Fly.prefab&Prefabs/Green/Effect_Skill_Green_05_Hit.prefab&Prefabs/Green/Effect_Skill_Green_05_Hit2.prefab&Prefabs/Green/Effect_Skill_Green_05_Hit3.prefab&Prefabs/Green/Effect_Skill_Green_05_Hit4.prefab&Prefabs/Green/Effect_Skill_Green_05_Loop.prefab&Prefabs/Green/Effect_Skill_Green_05_Start.prefab&Prefabs/Green/Effect_Skill_Green_05_Start2.prefab&Prefabs/Green/Effect_Skill_Green_06_Hit.prefab&Prefabs/Green/Effect_Skill_Green_06_Hit2.prefab&Prefabs/Green/Effect_Skill_Green_06_Loop.prefab&Prefabs/Green/Effect_Skill_Green_06_Start.prefab&Prefabs/Green/Effect_Skill_Green_07_Hit.prefab&Prefabs/Green/Effect_Skill_Green_08_Fly.prefab&Prefabs/Green/Effect_Skill_Green_08_Hit.prefab&Prefabs/Green/Effect_Skill_Green_08_Start.prefab&Prefabs/Green/Effect_Skill_Green_09_Hit1.prefab&Prefabs/Green/Effect_Skill_Green_09_Hit2.prefab&Prefabs/Green/Effect_Skill_Green_09_Start.prefab&Prefabs/Green/Effect_Skill_Green_10_Fly.prefab&Prefabs/Green/Effect_Skill_Green_10_Hit1.prefab&Prefabs/Green/Effect_Skill_Green_10_Hit2.prefab&Prefabs/Green/Effect_Skill_Green_10_Start.prefab&Prefabs/Green/Effect_Skill_Green_11_Fly.prefab&Prefabs/Green/Effect_Skill_Green_11_Hit.prefab&Prefabs/Green/Effect_Skill_Green_11_Start.prefab&Prefabs/Green/Effect_Skill_Green_12_Fly.prefab&Prefabs/Green/Effect_Skill_Green_12_Hit1.prefab&Prefabs/Green/Effect_Skill_Green_12_Hit2.prefab&Prefabs/Green/Effect_Skill_Green_12_Start.prefab&Prefabs/Green/Effect_Skill_Green_13_Fly.prefab&Prefabs/Green/Effect_Skill_Green_13_Hit.prefab&Prefabs/Green/Effect_Skill_Green_13_Hit2.prefab&Prefabs/Green/Effect_Skill_Green_13_Start.prefab&Prefabs/Green/Effect_Skill_Green_14_Hit.prefab&Prefabs/Green/Effect_Skill_Green_14_Start.prefab&Prefabs/Green/Effect_Skill_Green_15_Start.prefab&Prefabs/Green/Effect_Skill_Green_16_Fly.prefab&Prefabs/Green/Effect_Skill_Green_16_Hit.prefab&Prefabs/Green/Effect_Skill_Green_16_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_01_Hit.prefab&Prefabs/Purple/Effect_Skill_Purple_01_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_02_Fly.prefab&Prefabs/Purple/Effect_Skill_Purple_02_Hit.prefab&Prefabs/Purple/Effect_Skill_Purple_02_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_03_Fly.prefab&Prefabs/Purple/Effect_Skill_Purple_03_Hit.prefab&Prefabs/Purple/Effect_Skill_Purple_03_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_04_Hit.prefab&Prefabs/Purple/Effect_Skill_Purple_04_Loop.prefab&Prefabs/Purple/Effect_Skill_Purple_04_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_05_Loop.prefab&Prefabs/Purple/Effect_Skill_Purple_06_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_07_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_08_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_09_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_10_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_11_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_12_Hit.prefab&Prefabs/Purple/Effect_Skill_Purple_12_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_13_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_14_Fly.prefab&Prefabs/Purple/Effect_Skill_Purple_14_Hit.prefab&Prefabs/Purple/Effect_Skill_Purple_14_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_15_Fly.prefab&Prefabs/Purple/Effect_Skill_Purple_15_Hit.prefab&Prefabs/Purple/Effect_Skill_Purple_15_Start.prefab&Prefabs/Purple/Effect_Skill_Purple_16_Hit.prefab&Prefabs/Purple/Effect_Skill_Purple_16_Start.prefab&Prefabs/Snow/Effect_Skill_Snow_01_Hit.prefab&Prefabs/Snow/Effect_Skill_Snow_01_Start.prefab&Prefabs/Snow/Effect_Skill_Snow_02_Hit.prefab&Prefabs/Snow/Effect_Skill_Snow_02_Start.prefab&Prefabs/Snow/Effect_Skill_Snow_03_Hit.prefab&Prefabs/Snow/Effect_Skill_Snow_03_Loop.prefab&Prefabs/Snow/Effect_Skill_Snow_03_Start.prefab&Prefabs/Snow/Effect_Skill_Snow_04_Hit.prefab&Prefabs/Snow/Effect_Skill_Snow_04_Loop.prefab&Prefabs/Snow/Effect_Skill_Snow_04_Start.prefab&Prefabs/Snow/Effect_Skill_Snow_05_Fly.prefab&Prefabs/Snow/Effect_Skill_Snow_05_Hit.prefab&Prefabs/Snow/Effect_Skill_Snow_06_Fly.prefab&Prefabs/Snow/Effect_Skill_Snow_06_Hit.prefab&Prefabs/Snow/Effect_Skill_Snow_07_Hit.prefab&Prefabs/Snow/Effect_Skill_Snow_07_Start.prefab&Prefabs/TheBuff/Effect_Skill_Buff_01_Loop.prefab&Prefabs/TheBuff/Effect_Skill_Buff_01_Loop_Change1.prefab&Prefabs/TheBuff/Effect_Skill_Buff_01_Loop_Change2.prefab&Prefabs/TheBuff/Effect_Skill_Buff_01_Loop_Change3.prefab&Prefabs/TheBuff/Effect_Skill_Buff_03_Loop.prefab&Prefabs/TheBuff/Effect_Skill_Buff_03_Loop_Change1.prefab&Prefabs/TheBuff/Effect_Skill_Buff_03_Loop_Change2.prefab&Prefabs/TheBuff/Effect_Skill_Buff_04_Loop.prefab&Prefabs/TheBuff/Effect_Skill_Buff_04_Loop_Change1.prefab&Prefabs/TheBuff/Effect_Skill_Buff_05_Loop.prefab";
    private bool r = false;
    string[] allArray = null;

    public int i = 0;
    public UnityEngine.UI.Text tex;
    public Transform ts;
    private GameObject currObj;

    public void Awake()
    {

        /*string st2322r = "";
        var allFiles = Directory.GetFiles(Application.dataPath + "/RPG VFX Particles package 1/Resources", "*.prefab", SearchOption.AllDirectories);
        for (int i = 0; i < allFiles.Length; i++)
        {
            var str = Application.dataPath + "/RPG VFX Particles package 1/Resources/";
            allFiles[i] = allFiles[i].Replace(@"\", "/").Replace(str.Replace(@"\", "/"), "");
            st2322r += allFiles[i] + "&";

        }
        Debug.LogError(st2322r);
        return;*/


        allArray = ss.Split('&');
        currObj = GameObject.Instantiate(Resources.Load(allArray[i].ToLower().Replace(".prefab", "")) as GameObject);
        currObj.transform.SetParent(ts);
        currObj.transform.localPosition = Vector3.zero;
        tex.text = "Name: "+ i +" 【" + allArray[i] + "】";
    }

    public void Update()
    {
        if (ts != null && r)
        {
            ts.transform.Rotate(Vector3.up * Time.deltaTime * 90f);
        }
    }

    public void R()
    {
        r = true;
    }

    public void NotR()
    {
        r = false;
    }

    public void RePlay() 
    {
        if (currObj != null)
        {
            currObj.SetActive(false);
            currObj.SetActive(true);
        }
    }

    public void CopyName() 
    {
        var s = allArray[i].ToLower().Replace(".prefab", "");
        s = s.Substring(s.IndexOf("/")+1);
        UnityEngine.GUIUtility.systemCopyBuffer = s;
    }

    public void OnLeftBtClick() 
    {
        i--;
        if (i <= 0)
        {
            i = allArray.Length - 1;
        }
        if (currObj != null)
        {
            GameObject.DestroyImmediate(currObj);
        }
        currObj = GameObject.Instantiate(Resources.Load(allArray[i].ToLower().Replace(".prefab", "")) as GameObject);
        currObj.transform.SetParent(ts);
        currObj.transform.localPosition = Vector3.zero;
        tex.text = "Name: " + i + " 【" + allArray[i] + "】";
    }

    public void OnRightBtClick()
    {
        i++;
        if (i >= allArray.Length)
        {
            i = 0;
        }
        if (currObj != null)
        {
            GameObject.DestroyImmediate(currObj);
        }
        currObj = GameObject.Instantiate(Resources.Load(allArray[i].ToLower().Replace(".prefab", "")) as GameObject);
        currObj.transform.SetParent(ts);
        currObj.transform.localPosition = Vector3.zero;
        tex.text = "Name: " + i + " 【" + allArray[i] + "】";
    }
}
