using UnityEngine;
using static UnitMeta;

public interface IUnitMovement
{
    bool collided();
    void move(Vector3 position);
    void stop();
    void rotate(Quaternion targetRotation);
    void updateRotation(bool update);
    bool isCollide();
    bool hasArrived();
    Transform collideTargetTransform();
    float GetSpeed(SpeedType speedType);
    void SetSpeed(SpeedType speedType, float speed);
    Vector3 GetVelocity();
    void SetVelocity(Vector3 velocity);
    float GetRadius();
}
