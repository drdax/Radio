using System;
using System.ServiceModel;
using System.Windows;

namespace DrDax.RadioClient {
	/// <summary>Raidstaciju pārslēgšanas pakalpojuma kontrakts.</summary>
	[ServiceContract]
	public interface IRadioSwitch {
		/// <summary>Pārslēdz kanālu.</summary>
		/// <param name="id">Kanāla identifikators, kā tas padots programmas komandrindā.</param>
		[OperationContract]
		void SetChannel(string id);
	}
	/// <summary>Raidstaciju pārslēgšanas pakalpojums.</summary>
	[ServiceBehavior(IncludeExceptionDetailInFaults=true)]
	internal class RadioSwitch : IRadioSwitch {
		public const string ServiceUrl="net.pipe://localhost/RadioSwitch";
		public void SetChannel(string id) {
			// http://stackoverflow.com/questions/1906416/async-function-callback-using-object-owned-by-main-thread
			Application.Current.Dispatcher.InvokeAsync((Action)(async () => {
				try {
					await ((MainWindow)Application.Current.MainWindow).SetChannel(((RadioApp)Application.Current).GetChannel(id)).ConfigureAwait(false);
				} catch (Exception ex) {
					RadioApp.ShowError(ex.Message, "Kļūda pārslēdzot radio kanālu");
				}
			}));
		}
	}
}