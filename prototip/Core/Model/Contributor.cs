using Core.Utils;
using System;

namespace Core.Model
{
    public class Contributor(string contributionType, string personId, string workId)
    {
        private string contributionType = contributionType;
        private string personId = personId;
        private string workId = workId;

        public string ContributionType
        {
            get => contributionType;
            set => contributionType = value;
        }

        public string PersonId { get => personId; set => personId = value; }
        public string WorkId { get => workId; set => workId = value; }
    }
}
