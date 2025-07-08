using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;

public class LiveGunOriginScript : MonoBehaviour
{
    public int bulletAmount;//装弾数
    public float fireRate;//発射速度
    public float bulletSpeed;//弾速
    public float fireRange;//射程
    public float reloadTime;//リロード時間
    public int fireEnergyReq;//必要エネルギー(1発あたり)
    public int reloadEnergyReq;//必要エネルギー(リロード時)

    public bool isShotGun = false;//ショットガンかどうか

    bool isReload = false;//リロード中かどうか

    bool isRunningFire = false;//発射処理のコルーチンが動いているか

    bool isForcus = false;//TargetLookのコルーチンが動いているか

    public GameObject gunObj;//銃本体のオブジェクト
    public GameObject gunRootObj;//親オブジェクト

    GameObject targetEnemy;

    [SerializeField] GameObject bulletObj;//弾のプレハブオブジェクト
    public List<GameObject> unUsedBulletList = new List<GameObject>();//残弾用リスト
    public List<GameObject> usedBulletList = new List<GameObject>();//使用済みの弾用リスト

    public List<BulletScript> unUsedBulletSCList = new List<BulletScript>();//残弾のスクリプト用リスト
    public List<BulletScript> usedBulletSCList = new List<BulletScript>();//使用済み弾のスクリプト用リスト

    public EnergyScript energySC;//エネルギースクリプト
    public CoreScript coreSC;//コアスクリプト

    /// <summary>
    /// Startの変わりに継承先から呼ぶ準備関数
    /// </summary>
    public void Preparation()
    {
        gunObj = transform.parent.gameObject;
        gunRootObj = gunObj.transform.parent.gameObject;

        //弾プレハブを装弾数×2個分用意
        unUsedBulletList = BulletInst(bulletAmount);
        usedBulletList = BulletInst(bulletAmount * 2);


        //弾のスクリプト取得
        foreach (var list in unUsedBulletList)
        {
            unUsedBulletSCList.Add(list.GetComponent<BulletScript>());
        }
        foreach (var list in usedBulletList)
        {
            usedBulletSCList.Add(list.GetComponent<BulletScript>());
        }
    }


    /// <summary>
    /// 弾プレハブを装弾数分用意する
    /// </summary>
    /// <param name="amount">装弾数</param>
    List<GameObject> BulletInst(int amount)
    {
        var bulletList = new List<GameObject>();
        //弾プレハブを装弾数分用意
        for (int i = 0; i < amount; i++)
        {
            bulletList.Add(
            Instantiate(
                bulletObj,
                transform.position,
                Quaternion.identity
            ));
        }

        return bulletList;
    }


    ///<summary>
    ///球発射関数
    ///</summary>
    ///<param name="targetObj">ターゲットオブジェクト</param>
    public IEnumerator Fire(GameObject targetObj = null)
    {
        //コルーチン重複防止
        if (isRunningFire || isReload)
        {
            yield break;
        }

        isRunningFire = true;//処理始動

        if (!isForcus)//コルーチン処理中じゃなかったら
        {
            targetEnemy = targetObj;//ターゲットを取得
            StartCoroutine(TargetLook());//ターゲットに銃口を向ける
        }

        //残弾があれば撃つ
        if (unUsedBulletList.Count > 0 && energySC.UseEnergy(fireEnergyReq))
        {
            unUsedBulletList[0].transform.position = transform.position;
            unUsedBulletList[0].transform.rotation = transform.rotation;

            //残弾のスクリプトのShot関数呼び出し
            StartCoroutine(unUsedBulletSCList[0].Shot(bulletSpeed, fireRange));

            //撃ち出された弾とそのスクリプトを使用済みリストに追加
            usedBulletList.Add(unUsedBulletList[0]);
            usedBulletSCList.Add(unUsedBulletSCList[0]);


            //残弾リストから削除
            unUsedBulletList.RemoveAt(0);
            unUsedBulletSCList.RemoveAt(0);

            //ショットガンのみ
            if (isShotGun)
            {
                var currentAngle = transform.localEulerAngles;//角度保存

                //9発追加で発射
                for (int i = 0; i < 9; i++)
                {
                    //アングルをランダムに変えてばらけさせる
                    transform.localEulerAngles = new Vector3(Random.Range(-7.5f, 7.5f), Random.Range(-7.5f, 7.5f));
                    unUsedBulletList[0].transform.position = transform.position;
                    unUsedBulletList[0].transform.rotation = transform.rotation;

                    //残弾のスクリプトのShot関数呼び出し
                    StartCoroutine(unUsedBulletSCList[0].Shot(bulletSpeed, fireRange));

                    //撃ち出された弾とそのスクリプトを使用済みリストに追加
                    usedBulletList.Add(unUsedBulletList[0]);
                    usedBulletSCList.Add(unUsedBulletSCList[0]);


                    //残弾リストから削除
                    unUsedBulletList.RemoveAt(0);
                    unUsedBulletSCList.RemoveAt(0);

                    transform.localEulerAngles = currentAngle;//角度を戻す
                }
            }

        }
        else
        {
            //リロード中でなければリロード
            if (!isReload && energySC.UseEnergy(reloadEnergyReq))
            {
                StartCoroutine(Reload());
                isReload = true;
            }
        }
        yield return new WaitForSeconds(1f / fireRate);//発射間隔分待つ

        isRunningFire = false;//処理終了

    }

    /// <summary>
    /// リロード関数
    /// </summary>
    IEnumerator Reload()
    {
        yield return new WaitForSeconds(reloadTime);//リロード時間

        //装弾数がマックスになるまでリストに追加
        while (unUsedBulletList.Count < bulletAmount && usedBulletList.Count > 0)
        {
            unUsedBulletList.Add(usedBulletList[0]);
            unUsedBulletSCList.Add(usedBulletSCList[0]);

            usedBulletList.RemoveAt(0);
            usedBulletSCList.RemoveAt(0);
        }

        isReload = false;
    }

    /// <summary>
    /// ターゲットに銃口を向ける
    /// </summary>
    IEnumerator TargetLook()
    {
        if (targetEnemy != null)
        {
            while (true)
            {
                Vector3 targetDir = targetEnemy.transform.position - gunRootObj.transform.position;//ターゲットの方向
                float angle = Vector3.Angle(targetDir, gunRootObj.transform.forward);//銃本体とターゲットの方向の差分
                Quaternion targetRot = Quaternion.LookRotation(targetDir.normalized);//ターゲットの方向までの角度

                //差分が22.5度以下だったら
                if (angle <= 22.5f)
                {
                    isForcus = true;

                    //ターゲットの方向まで滑らかに回転
                    gunObj.transform.rotation = Quaternion.Slerp(
                        gunObj.transform.rotation,
                        targetRot,
                        0.2f
                        );
                }
                else
                {
                    isForcus = false;
                    break;
                }
                yield return null;//1フレーム待つ
            }
        }
    }
}
