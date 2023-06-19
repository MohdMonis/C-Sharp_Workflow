using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace CalculateIncomeTax
{
    public class Innoease_Workflows : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
                IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
                IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);
                ITracingService tracer = context.GetExtension<ITracingService>();

                tracer.Trace("Inside CalculateItemTax Workflow...");

                Guid recordGuid = Guid.Empty;
                decimal annualIncome;
                decimal incomeTax = 0;
                recordGuid = workflowContext.PrimaryEntityId;
                tracer.Trace("RecordGuid: " + recordGuid);

                Entity citizen = service.Retrieve("mon_citizen", recordGuid, new ColumnSet("mon_annualincome"));
                if (citizen != null && citizen.Contains("mon_annualincome"))
                {
                    annualIncome = ((Money)citizen["mon_annualincome"]).Value;
                    tracer.Trace("annual income: " + annualIncome);
                    if (annualIncome <= 450000)
                    {
                        incomeTax = ((10 * annualIncome) / 100);
                    }
                    else if (annualIncome > 450000 && annualIncome <= 750000)
                    {
                        incomeTax = ((15 * annualIncome) / 100);
                    }
                    else if (annualIncome > 750000)
                    {
                        incomeTax = ((20 * annualIncome) / 100);
                    }
                    tracer.Trace("income tax: " + incomeTax);
                    Entity citizenEntity = new Entity("mon_citizen");
                    citizenEntity.Id = recordGuid;
                    citizenEntity["mon_incometax"] = new Money(incomeTax);
                    service.Update(citizenEntity);
                }
                //throw new Exception("Custom Exception: ");
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException("CalculateIncomeTax|Error: " + e.Message);
            }
        }
    }
}