using UnityEngine;
using UnityEngine.UI;

public class EnergyBarManager : MonoBehaviour
{
	[Header("Energy Bar Components")]
	public Slider energyBarSlider;
	public Gradient energyBarGradient;
	public Image fillImage;

	private int maxWeight;

	public void InitializeEnergyBar(int maxWeight)
	{
		this.maxWeight = maxWeight;

		if (energyBarSlider != null)
		{
			energyBarSlider.maxValue = maxWeight;
			energyBarSlider.value = maxWeight; // Start full

			if (fillImage != null && energyBarGradient != null)
			{
				fillImage.color = energyBarGradient.Evaluate(1f); // Set full color
			}
		}
	}

	public void UpdateEnergyBar(int currentWeightUsed)
	{
		if (energyBarSlider != null)
		{
			float remainingEnergy = maxWeight - currentWeightUsed;
			energyBarSlider.value = remainingEnergy; // Deplete bar

			if (fillImage != null && energyBarGradient != null)
			{
				float normalizedValue = remainingEnergy / maxWeight; // Normalize between 0 and 1
				fillImage.color = energyBarGradient.Evaluate(normalizedValue);
			}
		}
	}
}
