using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace Innoease_Workflows
{
    public class SetTotalAndAvgItemTaxOnCitizen : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
                IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
                IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);
                ITracingService tracer = context.GetExtension<ITracingService>();

                tracer.Trace("Inside SetTotalAndAvgItemTaxOnCitizen Workflow...");

                Guid recordGuid = Guid.Empty;
                Guid cusRecordGuid = Guid.Empty;
                decimal totalGst = 0;
                decimal avgGst = 0;
                recordGuid = workflowContext.PrimaryEntityId;
                tracer.Trace("RecordGuid: " + recordGuid);

                Entity bill = service.Retrieve("mon_bill", recordGuid, new ColumnSet("new_itemtax", "mon_citizen"));
                if (bill != null && bill.Contains("mon_citizen") && bill.Contains("new_itemtax"))
                {
                    cusRecordGuid = ((EntityReference)bill["mon_citizen"]).Id;
                    tracer.Trace("bill ID: " + recordGuid + " CustomerID: " + cusRecordGuid);
                    totalGst = GetSumOfItemTax(service, tracer, cusRecordGuid);
                    avgGst = GetAvgOfItemTax(service, tracer, cusRecordGuid);
                    Entity citizenEntity = new Entity("mon_citizen");
                    citizenEntity.Id = cusRecordGuid;
                    citizenEntity["mon_totalgst"] = new Money(totalGst);
                    citizenEntity["mon_avggst"] = new Money(avgGst);
                    tracer.Trace("ctzid: " + cusRecordGuid + " recguid: " + recordGuid + " avg/total: " + avgGst + " / " + totalGst);
                    service.Update(citizenEntity);
                }
                //throw new Exception("Custom Exception: ");
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException("SetTotalAndAvgItemTaxOnCitizen|Error: " + e.Message);
            }
        }

        public decimal GetAvgOfItemTax(IOrganizationService service, ITracingService tracer, Guid recordId)
        {
            tracer.Trace("Inside GetAvgOfItemTax method...");
            string query = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' aggregate='true' distinct='false'>
                               <entity name='mon_bill'>
                                 <attribute name='new_itemtax' alias='itemtax_avg' aggregate='avg' />
                                 <filter type='and'>
                                   <condition attribute='mon_citizen' operator='eq' value='{0}' />
                                 </filter>
                               </entity>
                             </fetch>";
            query = string.Format(query, recordId);
            decimal itemTaxAvg = 0;
            EntityCollection bills = service.RetrieveMultiple(new FetchExpression(query));
            tracer.Trace("Entity Count: " + bills.Entities.Count);
            if (bills.Entities.Count > 0)
            {
                if (bills.Entities[0].Contains("itemtax_avg") && ((AliasedValue)bills.Entities[0]["itemtax_avg"]).Value != null)
                    itemTaxAvg = ((Money)((AliasedValue)bills.Entities[0]["itemtax_avg"]).Value).Value;
                tracer.Trace("Avg: " + itemTaxAvg);
            }
            return itemTaxAvg;
        }
        public decimal GetSumOfItemTax(IOrganizationService service, ITracingService tracer, Guid recordId)
        {
            tracer.Trace("Inside GetSumOfItemTax method...");
            string query = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' aggregate='true' distinct='false'>
                               <entity name='mon_bill'>
                                 <attribute name='new_itemtax' alias='itemtax_sum' aggregate='sum' />
                                 <filter type='and'>
                                   <condition attribute='mon_citizen' operator='eq' value='{0}' />
                                 </filter>
                               </entity>
                             </fetch>";
            query = string.Format(query, recordId);
            decimal itemTaxSum = 0;
            EntityCollection bills = service.RetrieveMultiple(new FetchExpression(query));
            tracer.Trace("Entity Count: " + bills.Entities.Count);
            if (bills.Entities.Count > 0)
            {
                if (bills.Entities[0].Contains("itemtax_sum") && ((AliasedValue)bills.Entities[0]["itemtax_sum"]).Value != null)
                    itemTaxSum = ((Money)((AliasedValue)bills.Entities[0]["itemtax_sum"]).Value).Value;
                tracer.Trace("Sum: " + itemTaxSum);
            }
            return itemTaxSum;
        }
    }
}