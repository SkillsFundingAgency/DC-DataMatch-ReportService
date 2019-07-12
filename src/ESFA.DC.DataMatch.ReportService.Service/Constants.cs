using System;

namespace ESFA.DC.DataMatch.ReportService.Service
{
    public static class Constants
    {
        public const string EmploymentStatusMonitoringTypeSEM = "SEM";

        // LearningDelivery FAM Codes
        public const string LearningDeliveryFAMCodeLSF = "LSF";
        public const string LearningDeliveryFAMCodeLDM = "LDM";
        public const string LearningDeliveryFAMCodeRES = "RES";
        public const string LearningDeliveryFAMCodeACT = "ACT";
        public const string LearningDeliveryFAMCodeSOF = "SOF";

        public const string LearningDeliveryFAMCode107 = "107";

        // learner FAM codes
        public const string LearnerFAMCodeEHC = "EHC";
        public const string LearnerFAMCodeHNS = "HNS";

        // FM25 FundLines
        public const string DirectFundedStudents1416FundLine = "14-16 Direct Funded Students";
        public const string Students1619FundLine = "16-19 Students (excluding High Needs Students)";
        public const string Students1619AllFundLine = "16-19 Students (including High Needs Students)";
        public const string HighNeedsStudents1619FundLine = "16-19 High Needs Students";
        public const string StudentsWithEHCP1924FundLine = "19-24 Students with an EHCP";
        public const string ContinuingStudents19PlusFundLine = "19+ Continuing Students (excluding EHCP)";

        // FM35 FundLines
        public const string Apprenticeship1618 = "16-18 Apprenticeship";
        public const string Apprenticeship1923 = "19-23 Apprenticeship";
        public const string Apprenticeship24Plus = "24+ Apprenticeship";
        public const string Traineeship1924NonProcured = "19-24 Traineeship (non-procured)";
        public const string Traineeship1924ProcuredFromNov2017 = "19-24 Traineeship (procured from Nov 2017)";
        public const string AebOtherLearningNonProcured = "AEB - Other Learning (non-procured)";
        public const string AebOtherLearningProcuredFromNov2017 = "AEB - Other Learning (procured from Nov 2017)";

        // FM25 Attributes
        public const string Fm25OnProgrammeAttributeName = "LnrOnProgPay";

        // FM35 Attributes
        public const string Fm35OnProgrammeAttributeName = "OnProgPayment";
        public const string Fm35BalancingAttributeName = "BalancePayment";
        public const string Fm35JobOutcomeAchievementAttributeName = "EmpOutcomePay";
        public const string Fm35AimAchievementAttributeName = "AchievePayment";
        public const string Fm35LearningSupportAttributeName = "LearnSuppFundCash";
        public const string Fm35AimAchievementPercentAttributeName = "AchievePayPct";

        // FM36 Attributes
        public const string Fm36PriceEpisodeOnProgPaymentAttributeName = "PriceEpisodeOnProgPayment";
        public const string Fm3PriceEpisodeBalancePaymentAttributeName = "PriceEpisodeBalancePayment";
        public const string Fm36PriceEpisodeCompletionPaymentAttributeName = "PriceEpisodeCompletionPayment";
        public const string Fm36LearnSuppFundCashAttributeName = "LearnSuppFundCash";
        public const string Fm36PriceEpisodeLSFCashAttributeName = "PriceEpisodeLSFCash";
        public const string Fm36MathEngOnProgPaymentAttributeName = "MathEngOnProgPayment";
        public const string Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName = "PriceEpisodeFirstDisadvantagePayment";
        public const string Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName = "PriceEpisodeSecondDisadvantagePayment";
        public const string Fm36PriceEpisodeFirstEmp1618PayAttributeName = "PriceEpisodeFirstEmp1618Pay";
        public const string Fm36PriceEpisodeSecondEmp1618PayAttributeName = "PriceEpisodeSecondEmp1618Pay";
        public const string Fm36PriceEpisodeFirstProv1618PayAttributeName = "PriceEpisodeFirstProv1618Pay";
        public const string Fm36PriceEpisodeSecondProv1618PayAttributeName = "PriceEpisodeSecondProv1618Pay";
        public const string Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName = "PriceEpisodeLearnerAdditionalPayment";
        public const string Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName = "PriceEpisodeApplic1618FrameworkUpliftOnProgPayment";
        public const string Fm36PriceEpisodeApplic1618FrameworkUpliftBalancingAttributeName = "PriceEpisodeApplic1618FrameworkUpliftBalancing";
        public const string Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName = "PriceEpisodeApplic1618FrameworkUpliftCompletionPayment";
        public const string Fm36ProgrammeAimOnProgPayment = "ProgrammeAimOnProgPayment";
        public const string Fm36ProgrammeAimBalPayment = "ProgrammeAimBalPayment";
        public const string Fm36ProgrammeAimCompletionPayment = "ProgrammeAimCompletionPayment";
        public const string Fm36ProgrammeAimProgFundIndMinCoInvest = "ProgrammeAimProgFundIndMinCoInvest";
        public const string Fm36MathEngOnProgPayment = "MathEngOnProgPayment";
        public const string Fm36MathEngBalPayment = "MathEngBalPayment";
        public const string Fm36LDApplic1618FrameworkUpliftBalancingPayment = "LDApplic1618FrameworkUpliftBalancingPayment";
        public const string Fm36LDApplic1618FrameworkUpliftCompletionPayment = "LDApplic1618FrameworkUpliftCompletionPayment";
        public const string Fm36LDApplic1618FrameworkUpliftOnProgPayment = "LDApplic1618FrameworkUpliftOnProgPayment";
        public const string Fm36DisadvFirstPayment = "DisadvFirstPayment";
        public const string Fm36DisadvSecondPayment = "DisadvSecondPayment";
        public const string Fm36LearnDelFirstProv1618Pay = "LearnDelFirstProv1618Pay";
        public const string Fm36LearnDelSecondProv1618Pay = "LearnDelSecondProv1618Pay";
        public const string Fm36LearnDelFirstEmp1618Pay = "LearnDelFirstEmp1618Pay";
        public const string Fm36LearnDelSecondEmp1618Pay = "LearnDelSecondEmp1618Pay";
        public const string Fm36LearnSuppFundCash = "LearnSuppFundCash";
        public const string Fm36LearnDelLearnAddPayment = "LearnDelLearnAddPayment";
        public const string Fm36PriceEpisodeContractTypeLevyContract = "Levy Contract";
        public const string Fm36LearnDelFAMTypeAct = "ACT";
        public const string Fm36LearnDelFAMCodeOne = "1";

        // FM81
        public const string Fm81CoreGovContPayment = "CoreGovContPayment";
        public const string Fm81MathEngBalPayment = "MathEngBalPayment";
        public const string Fm81MathEngOnProgPayment = "MathEngOnProgPayment";
        public const string Fm81AchPayment = "AchPayment";
        public const string Fm81SmallBusPayment = "SmallBusPayment";
        public const string Fm81YoungAppPayment = "YoungAppPayment";
        public const string Fm81LearnSuppFundCash = "LearnSuppFundCash";

        // Funding Summary
        public const string ALBInfoText = "Please note that loads bursary funding for learners who are funded within the Career Learning Pilot is not included here. Please refer to the separate Career Learning Pilot report.";

        // Exceptional Learning Support
        public const string ExceptionalLearningInfoText = "Exceptional learning support is paid out of a separate budget, not the budgets noted above. This is provided for information only and you will be informed separately of any payments made. Note payments are made following the last ILR collection of the funding year.";

        // ALB
        public const string ALBSupportPayment = "ALBSupportPayment";
        public const string AreaUpliftBalPayment = "AreaUpliftBalPayment";
        public const string AreaUpliftOnProgPayment = "AreaUpliftOnProgPayment";

        // Fundline types
        public const string AdvancedLearnerLoansBursary = "Advanced Learner Loans Bursary";
        public const string AdvancedLearnerLoansBursary_ExcessSupport = "Excess Support: Advanced Learner Loans Bursary";
        public const string AdvancedLearnerLoansBursary_AuthorisedClaims = "Authorised Claims: Advanced Learner Loans Bursary";

        public const string AEBOtherLearning = "AEB - Other Learning";
        public const string AEBOtherLearning_AuthorisedClaims = "Authorised Claims: AEB-Other Learning";
        public const string AEBOtherLearning_PrincesTrust = "Princes Trust: AEB-Other Learning";
        public const string AEBOtherLearning_ExcessLearningSupport = "Excess Learning Support: AEB-Other Learning";

        public const string Traineeships1924 = "19-24 Traineeships";
        public const string Traineeships1924_NonProcured = "19-24 Traineeship (non-procured)";
        public const string Traineeships1924_LearnerSupport = "Learner Support: 19-24 Traineeships";
        public const string Traineeships1924_ExcessLearningSupport = "Excess Learning Support: 19-24 Traineeships";
        public const string Traineeships1924_AuthorisedClaims = "Authorised Claims: 19-24 Traineeships";

        // Dates
        public const string Year = "2018/19";

        public const string FundingSummaryReportDecimalFormat = "#,##0.00";

        // Value Provider
        public const string Zero = "0";
        public const string NotApplicable = "n/a";
        public static string DateTimeMin = DateTime.MinValue.ToString("dd/MM/yyyy");

        // Dates
        public static readonly DateTime BeginningOfYear = new DateTime(2018, 8, 1);
        public static readonly DateTime EndOfYear = new DateTime(2019, 7, 31, 23, 59, 59);
    }
}
