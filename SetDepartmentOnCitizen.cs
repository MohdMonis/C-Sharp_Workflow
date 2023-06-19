using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Innoease_Workflows
{
    public class SetDepartmentOnCitizen : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
                IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
                IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);
                ITracingService tracer = context.GetExtension<ITracingService>();

                tracer.Trace("Inside SetDepartmentOnCitizen Workflow...");

                Guid recordGuid = Guid.Empty;
                recordGuid = workflowContext.PrimaryEntityId;
                tracer.Trace("RecordGuid: " + recordGuid);

                Entity citizen = service.Retrieve("mon_citizen", recordGuid, new ColumnSet("mon_jobtype"));
                if (citizen != null && citizen.Contains("mon_jobtype"))
                {
                    OptionSetValue optionSetValues = (OptionSetValue)citizen["mon_jobtype"];
                    tracer.Trace("job value: " + optionSetValues.Value);
                    OptionSetValueCollection osvals = new OptionSetValueCollection();
                    if (optionSetValues.Value == 180720000)
                    {
                        tracer.Trace("manager");
                        OptionSetValue val1 = new OptionSetValue(180720000);
                        osvals.Add(val1);
                        OptionSetValue val2 = new OptionSetValue(180720001);
                        osvals.Add(val2);
                        tracer.Trace("opsionset count: " + osvals.Count);
                    }

                    else if (optionSetValues.Value == 180720001)
                    {
                        tracer.Trace("superviser");
                        OptionSetValue val1 = new OptionSetValue(180720003);
                        osvals.Add(val1);
                        OptionSetValue val2 = new OptionSetValue(180720004);
                        osvals.Add(val2);
                        tracer.Trace("opsionset count: " + osvals.Count);
                    }

                    else if (optionSetValues.Value == 180720002)
                    {
                        tracer.Trace("director");
                        OptionSetValue val1 = new OptionSetValue(180720000);
                        osvals.Add(val1);
                        OptionSetValue val2 = new OptionSetValue(180720002);
                        osvals.Add(val2);
                        OptionSetValue val3 = new OptionSetValue(180720003);
                        osvals.Add(val3);
                        tracer.Trace("opsionset count: " + osvals.Count);
                    }
                    tracer.Trace("department: " + osvals.Count);
                    OptionSetValueCollection newValues = new OptionSetValueCollection();
                    foreach (var os in osvals)
                    {
                        newValues.Add(new OptionSetValue(os.Value));
                    }

                    tracer.Trace("new department: " + newValues.Count);
                    Entity citizenEn = new Entity("mon_citizen");
                    citizenEn.Id = recordGuid;
                    citizenEn["mon_department"] = newValues;
                    service.Update(citizenEn);
                }
                //throw new Exception("Custom Exception");
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException("SetDepartmentOnCitizen|Error: " + e.Message);
            }
        }
    }
}
