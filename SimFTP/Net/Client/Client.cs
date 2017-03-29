using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Cryptography;


using SimFTP.Config;
using SimFTP.Net.MetadataPackets;
using SimFTP.Net.DataPackets;
using SimFTP.Security;

using System.Net;
using System.IO;
using System.Net.Sockets;


namespace SimFTP.Net.Client
{
	public class Client : IDisposable
	{
		private readonly string ServerAddress;
		private const int ServerPort = 44335;
		private Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		public PacketType SendType { get; private set; }

		public Client(string address, PacketType sendingType)
		{
			clientSocket.NoDelay = false;
			clientSocket.SendBufferSize = int.MaxValue / 8;
			ServerAddress = address;

			if(sendingType == PacketType.BasicFrame || sendingType == PacketType.BasicSecurity || sendingType == PacketType.ExpertSecurity)
				SendType = sendingType;
			else
				throw new ArgumentException(string.Format("Try wrong packet type. {0}", sendingType));
		}

		public void SendFile(params BasicDataPacket[] files)
		{
			if(SendType == PacketType.BasicFrame)
			{
				clientSocket.Connect(ServerAddress, ServerPort);
				SendHandlingBasicPackets(files);
			}
			else
				ThrowFormattedException(PacketType.BasicFrame);
		}

		public void SendFile(string username, string password, params BasicSecurityDataPacket[] files)
		{
			if(SendType == PacketType.BasicSecurity)
			{
				clientSocket.Connect(ServerAddress, ServerPort);
				SendHandlingBasicSecurityPacket(username, password, files);
			}
			else
				ThrowFormattedException(PacketType.BasicSecurity);
		}

		public void SendFile(params BasicSecurityDataPacket[] files)
		{
			if(SendType == PacketType.BasicSecurity)
			{
				clientSocket.Connect(ServerAddress, ServerPort);
				SendHandlingBasicSecurityPacket(files);
			}
			else
				ThrowFormattedException(PacketType.BasicSecurity);
		}

		public void SendFile(params ExpertSecurityDataPacket[] files)
		{
			if(SendType == PacketType.ExpertSecurity)
			{
				clientSocket.Connect(ServerAddress, ServerPort);
				SendHandlingExpertSecurityPacket(files);
			}
			else
				ThrowFormattedException(PacketType.ExpertSecurity);
		}

		public void SendFile(string username, string password, params ExpertSecurityDataPacket[] files)
		{
			if(SendType == PacketType.ExpertSecurity)
			{
				clientSocket.Connect(ServerAddress, ServerPort);
				SendHandlingExpertSecurityPacket(username, password, files);
			}
			else
				ThrowFormattedException(PacketType.ExpertSecurity);
		}

		private void ThrowFormattedException(PacketType tryType)
		{
			throw new InvalidOperationException(string.Format("Wrong Packet Type. Tyring sending {0} type packet But, class sending type is {1}.", tryType, SendType));
		}

		private void SendHandlingBasicPackets(params BasicDataPacket[] files)
		{
			clientSocket.Send(new BasicMetadataPacket(files.Length).GetBinaryData());

			InfoPacket infoPacket = InfoExchangeHandler();

			// TO DO : CALL INFO PACKET RECEIVE EVENT. (Arg : InfoPacket) //

			//
			FileSendHandler(files);

			
		}

		// Anonymous Send //
		private void SendHandlingBasicSecurityPacket(params BasicSecurityDataPacket[] files)
		{
			clientSocket.Send(new BasicSecurityMetadataPacket(files.Length).GetBinaryData());

			InfoPacket infoPacket = InfoExchangeHandler();
			// TO DO : CALL INFO PACKET RECEIVE EVENT. (Arg : InfoPacket) //

			//

			FileSendHandler(files);
		}

		// User Auth Send //
		private void SendHandlingBasicSecurityPacket(string username, string password, params BasicSecurityDataPacket[] files)
		{
			clientSocket.Send(new BasicSecurityMetadataPacket(username, password, files.Length).GetBinaryData());
			InfoPacket infoPacket = InfoExchangeHandler();

			// TO DO : CALL INFO PACKET RECEIVE EVENT. (Arg : InfoPacket) //

			//

			FileSendHandler(files);
		}

		// Anonymous Encrypted Send //
		private void SendHandlingExpertSecurityPacket(params ExpertSecurityDataPacket[] files)
		{
			HandlingExpertSecurityExchange(files);
		}

		private void SendHandlingExpertSecurityPacket(string username, string password, params ExpertSecurityDataPacket[] files)
		{
			HandlingExpertSecurityExchange(files, username, password);
		}

		private void HandlingExpertSecurityExchange(ExpertSecurityDataPacket[] files, string username = null, string password = null)
		{
			using(DH521Manager dhManager = new DH521Manager())
			{
				if(username != null && password != null)
					clientSocket.Send(new ExpertSecurityMetadataPacket(files.Length, username, password, dhManager.PublicKey).GetBinaryData());
				else
					clientSocket.Send(new ExpertSecurityMetadataPacket(files.Length, dhManager.PublicKey).GetBinaryData());

				InfoPacket infoPacket = InfoExchangeHandler();
				ShareNetUtil.SendInfoPacket(clientSocket, InfoType.ShareKey, dhManager.PublicKey);
				byte[] shareKey = dhManager.GetShareKey(infoPacket.ResponseData);

				using(AES256Manager aesManager = new AES256Manager(shareKey))
				{
					const string EncryptTemp = ".encrypted";
					List<BasicDataPacket> lists = new List<BasicDataPacket>();
					foreach(var file in files)
					{
						using(CryptoStream encryptStream = aesManager.GetEncryptStream(File.Open(file.FileName + EncryptTemp, FileMode.Create)))
						{
							using(BinaryReader reader = new BinaryReader(file.File))
							{
								byte[] fileBuffer = new byte[int.MaxValue / 8];
								int readedBytes;
								long leftFile = file.File.Length;

								while(leftFile > fileBuffer.Length)
								{
									readedBytes = reader.Read(fileBuffer, 0, fileBuffer.Length);
									encryptStream.Write(fileBuffer, 0, readedBytes);

									leftFile -= readedBytes;
								}

								while(leftFile > 0)
								{
									readedBytes = reader.Read(fileBuffer, 0, (int)leftFile);
									encryptStream.Write(fileBuffer, 0, readedBytes);
									leftFile -= readedBytes;
								}
							}
						}

						using(BinaryReader reader = new BinaryReader(File.Open(file.FileName + EncryptTemp, FileMode.Open)))
						{
							file.SetFileSize(reader.BaseStream.Length);

							clientSocket.Send(file.GetOnlyBasicDataPacket());

							byte[] fileBuffer = new byte[int.MaxValue / 8];
							int readedBytes;
							long leftFile = reader.BaseStream.Length;

							while(leftFile > fileBuffer.Length)
							{
								readedBytes = reader.Read(fileBuffer, 0, fileBuffer.Length);
								clientSocket.Send(fileBuffer, readedBytes, SocketFlags.None);
								leftFile -= readedBytes;
							}

							while(leftFile > 0)
							{
								readedBytes = reader.Read(fileBuffer, 0, (int)leftFile);
								clientSocket.Send(fileBuffer, readedBytes, SocketFlags.None);
								leftFile -= readedBytes;
							}

							clientSocket.Send(file.GetBinaryData());
						}

						File.Delete(file.FileName + EncryptTemp);
					}

					clientSocket.Shutdown(SocketShutdown.Send);
				}
			}
		}

		private void FileSendHandler(params BasicDataPacket[] files)
		{
			foreach(var file in files)
			{
				if (file.File == null)
					clientSocket.Send(file.GetBinaryData());
				else
				{
					clientSocket.Send(file.GetOnlyBasicDataPacket());
					long leftFileSize = file.FileSize;

					using(BinaryReader reader = new BinaryReader(file.File))
					{
						byte[] fileBuffer = new byte[int.MaxValue / 8];
						while(leftFileSize > 0)
						{
							if(leftFileSize > fileBuffer.Length)
							{
								int readByte = reader.Read(fileBuffer, 0, fileBuffer.Length);
								clientSocket.Send(fileBuffer, 0, readByte, SocketFlags.None);
								leftFileSize -= readByte;
							}
							else
							{
								int readByte = reader.Read(fileBuffer, 0, (int)leftFileSize);
								clientSocket.Send(fileBuffer, readByte, SocketFlags.None);
								leftFileSize = readByte;
							}
						}

						fileBuffer = null;
					}


					GC.Collect();
					clientSocket.Send(file.GetBinaryData());
					clientSocket.Shutdown(SocketShutdown.Send);
				}
			}
		}

		private InfoPacket InfoExchangeHandler()
		{
			InfoPacket receivedInfo = ShareNetUtil.ReceiveInfoPacket(clientSocket);

			if (receivedInfo.Info == InfoType.Accept || receivedInfo.Info == InfoType.ShareKey)
				return receivedInfo;
			else if(receivedInfo.Info == InfoType.Error)
			{
				ErrorPacket error = ShareNetUtil.ReceiveErrorPacket(clientSocket, receivedInfo);
				clientSocket.Close();

				throw new SecurityException(string.Format("Received Error Packet. ERROR CODE : {0}", error.ErrorType.ToString()));
			}
			else
			{
				clientSocket.Close();

				throw new SecurityException(string.Format("Received Othres Status Packet. PACKET CODE {0}", receivedInfo.PacketType.ToString()));
			}
		}

		public void Dispose()
		{
			if(clientSocket.Connected)
				clientSocket.Disconnect(false);

			clientSocket.Dispose();
			return;
		}
	}
}
