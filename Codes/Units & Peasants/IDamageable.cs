public interface IDamageable
{
    public bool TakeDamage(int damage, int armorPiercingDamage = 0);
    public void Die();
}