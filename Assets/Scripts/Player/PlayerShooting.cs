using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour
{
    public int damagePerShot = 20;//玩家每一枪的伤害
    public float timeBetweenBullets = 0.15f;//射速
    public float range = 100f;//子弹的射程


    private float timer = 0.15f;//计时器，用来计算是否达到射击时间
    private Ray shootRay = new Ray();//射击射线
    private RaycastHit shootHit;//返回击中物体
    private int shootableMask;//设置可以击中的东西
    private ParticleSystem gunParticles;//枪口火焰例子效果
    private LineRenderer gunLine;//枪的子弹射线
    private AudioSource gunAudio;//开枪音效
    private Light gunLight;//子弹光照效果
    private float effectsDisplayTime = 0.2f;//开枪效果持续时间
    private GameObject player;

    void Awake ()
    {
        shootableMask = LayerMask.GetMask ("Shootable");//返回可以击中对象的层
        gunParticles = GetComponent<ParticleSystem> ();
        gunLine = GetComponent <LineRenderer> ();
        gunAudio = GetComponent<AudioSource> ();
        gunLight = GetComponent<Light> ();
        player = GameObject.FindGameObjectWithTag("Player");
    }


    private void OnEnable()
    {
        EasyJoystick.On_JoystickMove += OnJoystickMove;
        EasyJoystick.On_JoystickMoveEnd += OnJoystickMoveEnd;
    }

    private void OnDisable()
    {
        EasyJoystick.On_JoystickMove -= OnJoystickMove;
        EasyJoystick.On_JoystickMoveEnd -= OnJoystickMoveEnd;
    }

    private void OnJoystickMove(MovingJoystick move)
    {
        if (move.joystickName != "Fire")
        {
            return;
        }

        float PositionX = move.joystickAxis.x;
        float PositionY = move.joystickAxis.y;

        if (PositionX != 0 || PositionY != 0)
        {
            player.transform.LookAt(new Vector3(transform.position.x + PositionX, 0, transform.position.z + PositionY));
            //Debug.Log("aaa");
        }
        else
        {
            
        }

        timer += Time.deltaTime;//更新计时器
        //如果时间大于每一枪间隔时间则射击
        if (timer >= timeBetweenBullets && Time.timeScale != 0)
        {
            Shoot();
        }
    }

    void OnJoystickMoveEnd(MovingJoystick move)
    {
        if (move.joystickName.Equals("Fire"))
        {
            
        }
    }

    private void OnButtonPress(string buttonName)
    {
        if(buttonName == "fire")
        {
            timer += Time.deltaTime;//更新计时器
            //如果时间大于每一枪间隔时间则射击
            if (timer >= timeBetweenBullets && Time.timeScale != 0)
            {
                Shoot();
            }
        }
    }

    private void OnButtonLeave(string buttonName)
    {
        if (buttonName == "fire")
        {

        }
    }

    void Update ()
    {
        //timer += Time.deltaTime;//更新计时器

        //如果按下开火键（鼠标左键）且时间大于每一枪间隔时间则射击
        /*
		if(Input.GetButton ("Fire1") && timer >= timeBetweenBullets && Time.timeScale != 0)
        {
            Shoot ();
        }

        //如果超过开枪效果持续时间，取消开枪效果
        if(timer >= timeBetweenBullets * effectsDisplayTime)
        {
            DisableEffects ();
        }
        */
    }

    //取消开枪效果
    public void DisableEffects ()
    {
        gunLine.enabled = false;
        gunLight.enabled = false;
    }

    //玩家射击函数
    void Shoot ()
    {
        timer = 0f;//每次开枪时重置计时器

        gunAudio.Play ();//播放开枪音效

        gunLight.enabled = true;//激活子弹光照效果

        //停止之前枪口火焰粒子效果并重新播放枪口火焰粒子效果
        gunParticles.Stop ();
        gunParticles.Play ();

        gunLine.enabled = true;//激活子弹射线
        gunLine.SetPosition (0, transform.position);//射线初始坐标为0，即枪口位置

        shootRay.origin = transform.position;//射线发射点为枪口位置
        shootRay.direction = transform.forward;//射线方向为枪口朝向，即Z轴正方向

        //如果子弹击中物体，其中shootableMask保证击中的物体都在可击中层中
        if(Physics.Raycast (shootRay, out shootHit, range, shootableMask))
        {
            EnemyHealth enemyHealth = shootHit.collider.GetComponent <EnemyHealth> ();
            //如果击中物体是敌人，则enemyHealth为非空，如果不是敌人（如障碍物）则为空
            if(enemyHealth != null)
            {
                enemyHealth.TakeDamage (damagePerShot, shootHit.point);//对敌人造成伤害，shootHit.point为击中点
            }
            gunLine.SetPosition (1, shootHit.point);//设置子弹射线的起始点和终止点
        }
        //如果没有击中物体
        else
        {
            gunLine.SetPosition (1, shootRay.origin + shootRay.direction * range);//设置子弹射线的起始点和终止点，终止点为射程最远处
        }

        Invoke("DisableEffects", 0.05f);
    }
}
