using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class MacineGunScript : LiveGunOriginScript
{
    void Start()
    {
        //MacineGun用にパラメーターを設定
        bulletAmount = 30;
        bulletSpeed = 15;
        fireRate = 6;
        fireRange = 450;
        reloadTime = 2;
        fireEnergyReq = 2;
        reloadEnergyReq = bulletAmount * fireEnergyReq / 3;

        Preparation();
    }

    void Update()
    {
        Vector3 angle = gunObj.transform.localEulerAngles;//銃本体の回転を取得

        //0～360になっているのを-180～180にする
        if (angle.y > 180)
        {
            angle.y = angle.y - 360;
        }
        if (angle.x > 180)
        {
            angle.x = angle.x - 360;
        }

        //X軸とY軸の回転を45度の範囲で制限
        angle.x = Mathf.Clamp(angle.x, -22.5f, 22.5f);
        angle.y = Mathf.Clamp(angle.y, -22.5f, 22.5f);

        gunObj.transform.localRotation = Quaternion.Euler(angle);//制限された角度を入れる


        if (Input.GetButton("Fire1"))
        {
            StartCoroutine(Fire());
        }
    }
}
