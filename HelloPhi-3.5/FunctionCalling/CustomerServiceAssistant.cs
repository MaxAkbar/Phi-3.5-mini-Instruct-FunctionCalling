using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloPhi35.FunctionCalling
{
    public class CustomerServiceAssistant
    {
        [Function("Please refund ticket id.", true)]
        [Description("Process the refund having a ticket id.")]
        public string ProcessRefund([NotNull] string ticketId)
        {
            return "Refund was processed here your refund id = 12345";
        }

        [Function("Here is my email address.", true)]
        [Description("If the customer provides an email then let's the purchase histroy.")]
        public string GetTicketId([NotNull] string email)
        {
            return "Here is your purchase history \r\nTicket id: TKT-123456\r\nTicket id: TKT-345678\r\nPlease select a ticket id to refund.";
        }

        [Function("I want a refund. I want a refund but I don't remember my ticket id.", true)]
        [Description("If the customer provides does not provide an email or a ticket id.")]
        public string NoMapping()
        {
            return "To refund a ticket please provide your email address.";
        }
    }
}
