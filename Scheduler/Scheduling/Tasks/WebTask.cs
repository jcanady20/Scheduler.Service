using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

using Scheduler.Logging;

namespace Scheduler.Scheduling.Tasks
{
	[DisplayName("Web Request Call")]
	[Description("Web Request Call")]
	public class WebTask : BaseTask
	{
		public WebTask()
		{
		}

		public override void OnExecute()
		{
			var url = m_taskStep.Command;
			try
			{
				var request = WebRequest.Create(url);
				request.Credentials = CredentialCache.DefaultCredentials;
				//	Fire a Get Request
				using (var resp = (HttpWebResponse)request.GetResponse())
				{
					if(resp.StatusCode == HttpStatusCode.OK)
					{
						WebTaskRespponseManager.Instance.AwaitResponse(m_taskStep.JobId, this.Id, m_taskStep.StepId, m_taskStep.Name);
					}
				}
			}
			catch(WebException we)
			{
				var msg = String.Format("Web Request failed: [{0}]: ", we.Status);
				
				#region Add Status Descriptions
				switch (we.Status)
				{
					case WebExceptionStatus.ConnectFailure:
						msg += "The remote service could not be contacted at the transport level.";
						break;
					case WebExceptionStatus.ConnectionClosed:
						msg += "The connection was closed prematurely.";
						break;
					case WebExceptionStatus.NameResolutionFailure:
						msg += "The name service could not resolve the host name.";
						break;
					case WebExceptionStatus.ProtocolError:
						msg += "The response received from the server was complete but indicated an error at the protocol level.";
						break;
					case WebExceptionStatus.ReceiveFailure:
						msg += "A complete response was not received from the remote server.";
						break;
					case WebExceptionStatus.RequestCanceled:
						msg += "The request was canceled.";
						break;
					case WebExceptionStatus.SecureChannelFailure:
						msg += "An error occurred in a secure channel link.";
						break;
					case WebExceptionStatus.ServerProtocolViolation:
						msg += "The server response was not a valid HTTP response.";
						break;
					case WebExceptionStatus.Timeout:
						msg += "No response was received within the time-out set for the request.";
						break;
					case WebExceptionStatus.TrustFailure:
						msg += "A server certificate could not be validated.";
						break;
					case WebExceptionStatus.MessageLengthLimitExceeded:
						msg += "A message was received that exceeded the specified limit when sending a request or receiving a response from the server.";
						break;
					case WebExceptionStatus.PipelineFailure:
						msg += "This value supports the .NET Framework infrastructure and is not intended to be used directly in your code.";
						break;
					case WebExceptionStatus.ProxyNameResolutionFailure:
						msg += "The name resolver service could not resolve the proxy host name.";
						break;
					case WebExceptionStatus.UnknownError:
						msg += "An exception of unknown type has occurred.";
						break;
				}
				#endregion
				
				throw new Exception(msg);
			}			
		}
	}
}
