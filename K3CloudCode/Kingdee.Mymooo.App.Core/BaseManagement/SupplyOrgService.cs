using Kingdee.BOS;
using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Core.Warn;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
using Kingdee.BOS.BusinessEntity.YunZhiJia;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Enums;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.Operation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.Mymooo.App.Core.KafkaProducer;
using Kingdee.Mymooo.Contracts.BaseManagement;
using Kingdee.Mymooo.Core;
using Kingdee.Mymooo.Core.ApigatewayConfiguration;
using Kingdee.Mymooo.Core.BaseManagement;
using Kingdee.Mymooo.Core.Common;
using Kingdee.Mymooo.Core.ProductionManagement;
using Kingdee.Mymooo.Core.SalesManagement;
using Kingdee.Mymooo.Core.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Kingdee.Mymooo.App.Core.BaseManagement
{
	/// <summary>
	/// 获取供货组织(货主)对应的事业部
	/// </summary>
	public class SupplyOrgService : ISupplyOrgService
	{
		public string GetSupplyOrgBusinessDivision(Context ctx, long supplyOrgId)
		{
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@SupplyOrgId", KDDbType.Int64, supplyOrgId) };
			var sql = $@"select top 1 FBUSINESSDIVISION from T_BD_SupplyOrgToBD where  FSUPPLYORGID=@SupplyOrgId";
			return DBServiceHelper.ExecuteScalar<string>(ctx, sql, "", paramList: pars.ToArray());
		}

		/// <summary>
		/// 发送华东五部的供应商信息
		/// </summary>
		/// <returns></returns>
		public void SenMesSupplyInfo(Context ctx, string supplierCode)
		{
			List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
			MesSupplyInfoEntity entity = new MesSupplyInfoEntity();
			List<SqlParam> pars = new List<SqlParam>() { new SqlParam("@SupplierCode", KDDbType.String, supplierCode) };
			var sql = $@"select top 1 FSUPPLIERID from T_BD_SUPPLIER where FNUMBER=@SupplierCode and FUseOrgId=7401803";
			long supplierId = DBServiceHelper.ExecuteScalar<long>(ctx, sql, 0, paramList: pars.ToArray());
			if (supplierId > 0)
			{
				//晚2个s,让事务可以提交成功后再获取消息
				System.Threading.Thread.Sleep(2000);

				pars = new List<SqlParam>() { new SqlParam("@SupplierId", KDDbType.Int64, supplierId) };
				sql = $@"select top 1 t1.FSUPPLIERID SupplierId,t1.FNUMBER Number,t2.FNAME Name,t2.FShortName ShortName,t1.FDocumentStatus DocumentStatus,t1.FForbidStatus ForbidStatus,t1.FIsTemporary IsTemporary,
                            t3.FAddress Address,t3.FWebSite WebSite,t3.FRegisterAddress RegisterAddress,t3.FLegalPerson LegalPerson,t3.FRegisterFund RegisterFund,
                            t3.FRegisterCode RegisterCode,t3.FTendPermit TendPermit,t3.FSupplyClassify SupplyClassify,t3.FSOCIALCRECODE SocialCreditCode,t1.FModifyDate ModifyDate
                            from T_BD_SUPPLIER t1
                            inner join T_BD_SUPPLIER_L t2 on t1.FSUPPLIERID=t2.FSUPPLIERID and t2.FLOCALEID=2052
                            left join T_BD_SUPPLIERBASE t3 on t2.FSUPPLIERID=t3.FSUPPLIERID
                            where t1.FSUPPLIERID=@SupplierId ";
				var datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
				foreach (var item in datas)
				{
					entity.SupplierId = Convert.ToInt64(item["SupplierId"]);
					entity.Number = Convert.ToString(item["Number"]);
					entity.Name = Convert.ToString(item["Name"]);
					entity.ShortName = Convert.ToString(item["ShortName"]);
					entity.DocumentStatus = Convert.ToString(item["DocumentStatus"]);
					entity.ForbidStatus = Convert.ToString(item["ForbidStatus"]);
					entity.IsTemporary = Convert.ToString(item["IsTemporary"]);
					entity.Address = Convert.ToString(item["Address"]);
					entity.WebSite = Convert.ToString(item["WebSite"]);
					entity.RegisterAddress = Convert.ToString(item["RegisterAddress"]);
					entity.LegalPerson = Convert.ToString(item["LegalPerson"]);
					entity.RegisterFund = Convert.ToDecimal(item["RegisterFund"]);
					entity.RegisterCode = Convert.ToString(item["RegisterCode"]);
					entity.TendPermit = Convert.ToString(item["TendPermit"]);
					entity.SupplyClassify = Convert.ToString(item["SupplyClassify"]);
					entity.SocialCreditCode = Convert.ToString(item["SocialCreditCode"]);
					entity.ModifyDate = Convert.ToDateTime(item["ModifyDate"]);
				}
				//联系人
				sql = $@"select FContactNumber ContactNumber,FContact Contact,FPost Post,FTel Tel,FMobile Mobile,FFax Fax,FEMail EMail,FIsDefault IsDefault,
                        FDescription Description,FFORBIDSTATUS ConForbidStatus
                        from T_BD_SUPPLIERCONTACT t1 where t1.FSUPPLIERID=@SupplierId ";
				datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
				List<MesSupplyContactDetEntity> contactDetList = new List<MesSupplyContactDetEntity>();
				foreach (var item in datas)
				{
					contactDetList.Add(new MesSupplyContactDetEntity
					{
						Contact = Convert.ToString(item["Contact"]),
						Post = Convert.ToString(item["Post"]),
						Tel = Convert.ToString(item["Tel"]),
						Mobile = Convert.ToString(item["Mobile"]),
						Fax = Convert.ToString(item["Fax"]),
						EMail = Convert.ToString(item["EMail"]),
						IsDefault = Convert.ToString(item["IsDefault"]),
						Description = Convert.ToString(item["Description"]),
						ConForbidStatus = Convert.ToString(item["ConForbidStatus"]),
						ContactNumber = Convert.ToString(item["ContactNumber"])
					});
				}
				entity.ContactDetails = contactDetList;
				//银行信息
				sql = $@"select t1.FSUPPLIERID,t2.FOPENBANKNAME OpenBankName,t1.FISDEFAULT IsDefault,t1.FBankHolder BankAccountName,t1.FBankCode BankCode,t1.FOpenAddressRec OpenBankAddress from t_BD_SupplierBank t1
						left join T_BD_SUPPLIERBANK_L t2 on t1.FBANKID=t2.FBANKID and t2.FLOCALEID=2052
						inner join T_BD_SUPPLIER t3 on t1.FSUPPLIERID=t3.FSUPPLIERID	
						where t1.FSUPPLIERID=@SupplierId ";
				datas = DBServiceHelper.ExecuteDynamicObject(ctx, sql, paramList: pars.ToArray());
				List<MesSupplyBankEntity> bankDetList = new List<MesSupplyBankEntity>();
				foreach (var item in datas)
				{
					bankDetList.Add(new MesSupplyBankEntity
					{
						OpenBankName = Convert.ToString(item["OpenBankName"]),
						BankAccountName = Convert.ToString(item["BankAccountName"]),
						BankCode = Convert.ToString(item["BankCode"]),
						OpenBankAddress = Convert.ToString(item["OpenBankAddress"]),
						IsDefault = Convert.ToString(item["IsDefault"])
					});
				}
				entity.BankDetails = bankDetList;
				entity.FormId = "BD_Supplier";
				messages.Add(new RabbitMQMessage
				{
					Exchange = "baseManagement",
					Routingkey = entity.FormId,
					Keyword = entity.Number,
					Message = JsonConvertUtils.SerializeObject(entity)
				});

				KafkaProducerService kafkaProducer = new KafkaProducerService();
				kafkaProducer.AddMessage(ctx, messages.ToArray());

				Task.Factory.StartNew(() =>
				{
					//晚2个s在发送消息
					System.Threading.Thread.Sleep(2000);
					ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/RabbitMQ/SendMqMessage");
				});
			}

		}

		//供应商供应产品小类
		public ResponseMessage<dynamic> SupplierSmallService(Context ctx, SupplierSmallRequest[] requests)
		{
			ResponseMessage<dynamic> response = new ResponseMessage<dynamic>();
			if (requests.Count() > 0)
			{
				var sSql = "truncate table T_BD_SupplierSmall";
				DBServiceHelper.Execute(ctx, sSql);
			}
			MymoooBusinessDataService service = new MymoooBusinessDataService();
			var billView = FormMetadataUtils.CreateBillView(ctx, "PENY_BD_SupplierSmall");
			int rowIndex = 0;
			List<DynamicObject> dynamicObjects = new List<DynamicObject>();
			foreach (var request in requests)
			{
				if (rowIndex > 0)
				{
					billView.CreateNewModelData();
				}
				billView.Model.SetItemValueByNumber("FSupplierId", request.SupplierCode, 0);
				billView.Model.SetValue("FMaterialGroup", request.SmallId);
				dynamicObjects.Add(billView.Model.DataObject);
				rowIndex++;
			}
			var oper = service.SaveAndAuditBill(ctx, billView.BusinessInfo, dynamicObjects.ToArray());
			//清除释放网控
			billView.CommitNetworkCtrl();
			billView.InvokeFormOperation(FormOperationEnum.Close);
			billView.Close();
			if (!oper.IsSuccess)
			{
				if (oper.ValidationErrors.Count > 0)
				{
					response.Message += string.Join(";", oper.ValidationErrors.Select(p => p.Message));
					response.Code = ResponseCode.Exception;
				}
				else
				{
					response.Message += string.Join(";", oper.OperateResult.Where(p => !p.SuccessStatus).Select(p => p.Message));
					response.Code = ResponseCode.Exception;
				}
			}
			else
			{
				response.Code = ResponseCode.Success;
			}
			//初始化新增供应商+小类检验评分
			List<RabbitMQMessage> messages = new List<RabbitMQMessage>();
			messages.Add(new RabbitMQMessage
			{
				Exchange = "baseManagement",
				Routingkey = "SupplierClassInspectScore",
				Keyword = "SupplierClassInspectScore",
				Message = ""
			});
			KafkaProducerService kafkaProducer = new KafkaProducerService();
			kafkaProducer.AddMessage(ctx, messages.ToArray());
			Task.Factory.StartNew(() =>
			{
				//晚2个s在发送消息
				System.Threading.Thread.Sleep(2000);
				ApigatewayUtils.InvokeWebService($"k3cloudapi/{ApigatewayUtils.ApigatewayConfig.EnvCode}/RabbitMQ/SendMqMessage");
			});
			return response;
		}
	}

};