using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BulletScript : MonoBehaviour
{

    bool hit = false;//当たり判定用

    [SerializeField] float mass = 0;//弾丸質量
    int damage = 0;//ダメージ用
    float attenuation = 0;//減衰用

    [SerializeField] MeshRenderer meshRen;//非表示用

    [SerializeField] Collider col;//コライダー

    AnimationCurve speedAnimC;//減衰用のアニメーションカーブ
    AnimationCurve heightAnimC;//弾落ち用のアニメーションカーブ

    private void Start()
    {
        meshRen = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
    }

    /// <summary>
    /// 弾の挙動制御
    /// </summary>
    /// <param name="speed">弾速</param>
    /// <param name="range">射程</param>
    public IEnumerator Shot(float speed, float range)
    {
        meshRen.enabled = true;//弾表示
        col.enabled = true;//判定表示

        //距離に合わせて減衰用、弾落ちのアニメーションカーブを作成
        if (speedAnimC == null)
        {
            speedAnimC = new AnimationCurve();

            speedAnimC.AddKey(new Keyframe(range * 0.75f, speed));
            speedAnimC.AddKey(new Keyframe(range, speed * 0.5f, 0, 0));
        }

        if (heightAnimC == null)
        {
            heightAnimC = new AnimationCurve();

            heightAnimC.AddKey(new Keyframe(range * 0.5f, 0, 0, 0));
            heightAnimC.AddKey(new Keyframe(range, -0.5f * mass, 0, 0));
        }

        hit = true;//当たり判定ON

        float distance = 0f;//距離

        while (hit)
        {
            float eval = speedAnimC.Evaluate(distance);//現在位置の値

            attenuation = eval;

            float moveAmount = speed * eval * Time.deltaTime;//進む距離

            transform.position += transform.forward * moveAmount + new Vector3(0, heightAnimC.Evaluate(distance));//ポジション移動

            distance += moveAmount;//距離加算

            //距離が射程範囲を超えたら止める
            if (distance >= range)
                break;

            yield return null;//1フレーム待つ
        }

        BulletHit();//見た目と当たり判定非表示
    }

    /// <summary>
    /// ダメージ値を計算して返す
    /// </summary>
    public int GetDamage()
    {
        damage = (int)(mass * attenuation);
        return damage;
    }

    private void OnCollisionEnter(Collision collision)
    {
        BulletHit();//見た目と当たり判定非表示
    }

    /// <summary>
    /// 着弾時の処理(見た目と当たり判定非表示)
    /// </summary>
    void BulletHit()
    {
        hit = false;
        meshRen.enabled = false;
        col.enabled = false;
    }
}
