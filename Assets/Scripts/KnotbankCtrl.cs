using System;
using NMY;
using UnityEngine;
using UnityEngine.UI;

namespace DFKI.NMY
{
	public class KnotbankCtrl : StartupBehaviour
	{

		[SerializeField] private float maxValue=500;
		[SerializeField] private float minValue=-10;
		[SerializeField] private Image fillVisualization;
		
		private int tmpInt;
		private int tmpInt2;
		private ContactState contactVal;
		private int tensionVal;
		[SerializeField] private SerialController knotBankSerialController;
		[SerializeField] private float lerpSpeed = 0.1f;
		private bool messageReceived;

		protected override void StartupEnter()
		{
			tensionVal = 0;
			messageReceived = false;
			if (knotBankSerialController) {
				knotBankSerialController.SerialMessageEventHandler -= OnMessageArrived;
				knotBankSerialController.SerialMessageEventHandler += OnMessageArrived;
			}
			else
			{
				Debug.LogError("Serial Controller not referenced "+this.gameObject.name);
			}
		}

		private void Update()
		{
			if (fillVisualization)
			{
				// Lerp towards
				fillVisualization.fillAmount = Mathf.Lerp(fillVisualization.fillAmount, GetNormalizedTensionValue(),lerpSpeed);
			}
			
		}

		public int GetTensionValue() {
			return tensionVal;
		}

		public float GetNormalizedTensionValue()
		{
			
			
			// Check if the range is valid
			if (minValue >= maxValue)
			{
				throw new ArgumentException("Min value must be less than max value.");
			}
			
			// Normalize the value
			float normalizedValue = (Mathf.Abs(GetTensionValue()) - minValue) / (maxValue - minValue);

			// Ensure the result is within the [0, 1] range
			return Mathf.Max(0.0f, Mathf.Min(1.0f, normalizedValue));
		}
		
		public ContactState GetContactValue() {
			return contactVal;
		}
		
		
		// Invoked when a line of data is received from the serial device.
		private void OnMessageArrived(object sender, MessageEventArgs e) {
			string[] data = e.message.Split(';');
			if (int.TryParse(data[0], out tmpInt)) {
				if(tmpInt == 0 || tmpInt == 1)
					contactVal = (ContactState)tmpInt;
			}
			if (int.TryParse(data[1], out tmpInt2)) {
				tensionVal = -tmpInt2;
			}
			
			messageReceived = true;
			//Debug.Log("data: " + contactVal + " " + tensionVal);
		}

	}


}
