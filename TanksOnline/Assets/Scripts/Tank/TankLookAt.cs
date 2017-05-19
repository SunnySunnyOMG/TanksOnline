using UnityEngine;
using System.Collections;
using UnityStandardAssets.Utility;
using UnityEngine.Networking;

public class TankLookAt : NetworkBehaviour {

    //观察目标  
    public Transform Target;
    //观察距离  
    public float Distance = 5f;
    //旋转速度  
    public float SpeedX = 240;
    public float SpeedY = 120;
    //角度限制  
    public float MinLimitY = 5;
    public float MaxLimitY = 180;

    //旋转角度  
    private float mX = 0.0F;
    private float mY = 0.0F;

    //鼠标缩放距离最值  
    private float MaxDistance = 10;
    private float MinDistance = 1.5F;
    //鼠标缩放速率  
    private float ZoomSpeed = 2F;

    //是否启用差值  
    public bool isNeedDamping = true;
    //速度  
    public float Damping = 10F;

    private Quaternion mRotation;
    //identity
    private bool netView;
    /*
     * angle offset, this is very important when treating world rotation and local rotation
        because in this case, the camera is a child of the tank object
    */
    private float angleOffset;

    void Start()
    {

        angleOffset = transform.rotation.eulerAngles.y;
        //Debug.Log("angle offset: " + angleOffset);
       // Debug.Log("rotation:"+ transform.rotation.eulerAngles);

        //初始化旋转角度  
        mX = 0f;
        mY = 0f;
       // Debug.Log("mY: " + mY + "  mX: " + mX);

        //get identity
        netView = GetComponentInParent<NetworkIdentity>().isLocalPlayer;
    }

    void LateUpdate()
    {
        if (!netView)
            return;
        //鼠标右键旋转  
        if (Target != null)
        {
            //获取鼠标输入  
            mX += Input.GetAxis("Mouse X") * SpeedX * 0.02F;
            mY -= Input.GetAxis("Mouse Y") * SpeedY * 0.02F;
            //范围限制  
            mY = ClampAngle(mY, MinLimitY, MaxLimitY);
            //计算旋转  
            mRotation =  Quaternion.Euler(mY, mX + angleOffset, 0f);
            //根据是否插值采取不同的角度计算方式  
            if (isNeedDamping)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, mRotation, Time.deltaTime * Damping);
            }
            else
            {
                transform.rotation = mRotation;
            }

        }

        //鼠标滚轮缩放  
        Distance -= Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;
        Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);

        //重新计算位置  
        Vector3 mPosition = mRotation * new Vector3(0.0F, 0.0F, -Distance) + Target.position;

        //设置相机的角度和位置  
        if (isNeedDamping)
        {
            transform.position = Vector3.Lerp(transform.position, mPosition, Time.deltaTime * Damping);
        }
        else
        {
            transform.position = mPosition;
        }

    }


    //角度限制  
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}  

