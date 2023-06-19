using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace Innoease_Workflows
{
    public class SetPINOnCitizen : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
                IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
                IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);
                ITracingService tracer = context.GetExtension<ITracingService>();

                tracer.Trace("Inside SetPINOnCitizen Workflow...");

                Guid recordGuid = Guid.Empty;
                string pinNo = "";
                int countLocation = 0;
                int location = 0;
                recordGuid = workflowContext.PrimaryEntityId;
                tracer.Trace("RecordGuid: " + recordGuid);

                Entity citizen = service.Retrieve("mon_citizen", recordGuid, new ColumnSet("mon_location"));
                if (citizen != null && citizen.Contains("mon_location"))
                {
                    location = ((OptionSetValue)citizen["mon_location"]).Value;
                    countLocation = GetCountBasedOnLocation(service, tracer, location);
                    if (location == 180720000)
                    {
                        pinNo = "BPL_" + countLocation;
                    }
                    if (location == 180720001)
                    {
                        pinNo = "IND_" + countLocation;
                    }
                    if (location == 180720002)
                    {
                        pinNo = "JAB_" + countLocation;
                    }

                    tracer.Trace("PIN No: " + pinNo);
                    Entity citizenEntity = new Entity("mon_citizen");
                    citizenEntity.Id = recordGuid;
                    citizenEntity["mon_pinno"] = pinNo;
                    service.Update(citizenEntity);
                }
                //throw new Exception("Custom Exception: ");
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException("SetPINOnCitizen|Error: " + e.Message);
            }
        }

        public int GetCountBasedOnLocation(IOrganizationService service, ITracingService tracer, int location)
        {
            tracer.Trace("Inside GetCountByIdBasedOnLocation method...");
            string qry = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                             <entity name='mon_citizen'>
                               <attribute name='mon_name' />
                               <filter type='and'>
                                 <condition attribute='mon_location' operator='eq' value='{0}' />
                               </filter>
                             </entity>
                           </fetch>";
            qry = string.Format(qry, location);
            int locationCount = 0;
            EntityCollection entityCol = service.RetrieveMultiple(new FetchExpression(qry));
            tracer.Trace("location: " + location + " Initial Count: " + locationCount);
            if (entityCol.Entities.Count > 0)
            {
                locationCount = entityCol.Entities.Count;
            }
            tracer.Trace("Count: " + locationCount);
            return locationCount;
        }
    }
}