using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace Innoease_Workflows
{
    public class SetDesignationOnCitizen : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
                IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
                IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);
                ITracingService tracer = context.GetExtension<ITracingService>();

                tracer.Trace("Inside SetDesignationOnCitizen Workflow...");

                Guid recordGuid = Guid.Empty;
                recordGuid = workflowContext.PrimaryEntityId;
                tracer.Trace("RecordGuid: " + recordGuid);

                Entity citizen = service.Retrieve("mon_citizen", recordGuid, new ColumnSet("mon_expertise"));
                if (citizen != null && citizen.Contains("mon_expertise"))
                {
                    OptionSetValueCollection optionSetValues = (OptionSetValueCollection)citizen["mon_expertise"];

                    string designation;
                    if (optionSetValues.Contains(new OptionSetValue(180720000)) && optionSetValues.Contains(new OptionSetValue(180720001)) && optionSetValues.Contains(new OptionSetValue(180720002)))
                        designation = "Web Developer";
                    
                    else if (optionSetValues.Contains(new OptionSetValue(180720000)) && optionSetValues.Contains(new OptionSetValue(180720001)))
                        designation = "Dot Net Developer";
                    
                    else if (optionSetValues.Contains(new OptionSetValue(180720001)) && optionSetValues.Contains(new OptionSetValue(180720002)))
                        designation = "Database Administrator";
                    
                    else
                        designation = "Software Trainee";

                    Entity citizenEn = new Entity("mon_citizen");
                    citizenEn.Id = recordGuid;
                    citizenEn["mon_designation"] = designation;
                    service.Update(citizenEn);
                }
                //throw new Exception("Custom Exception");
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException("SetDesignationOnCitizen|Error: " + e.Message);
            }
        }
    }
}
