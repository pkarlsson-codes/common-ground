using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.PropertyRegistry.Domain.Homes;

namespace CommonGround.PropertyRegistry.Application.Errors;

public static class BusinessRuleViolation
{
    public static class LandParcel
    {
        public static class Register
        {
            public static BusinessRuleViolationException LegalParcelNumberRequired()
                => new(
                    "property_registry.land_parcel.register.legal_parcel_number_required");

            public static BusinessRuleViolationException AddressRequired()
                => new(
                    "property_registry.land_parcel.register.address_required");

            public static BusinessRuleViolationException AreaNonPositive(decimal areaAcres)
                => new(
                    "property_registry.land_parcel.register.area_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["areaAcres"] = areaAcres,
                    });
        }

        public static class ListForPlacement
        {
            public static BusinessRuleViolationException ParcelDecommissioned()
                => new(
                    "property_registry.land_parcel.list_for_placement.parcel_decommissioned");
        }

        public static class WithdrawForRedevelopment
        {
            public static BusinessRuleViolationException ParcelDecommissioned()
                => new(
                    "property_registry.land_parcel.withdraw_for_redevelopment.parcel_decommissioned");
        }
    }

    public static class Home
    {
        public static class Register
        {
            public static BusinessRuleViolationException BedroomsNegative(int bedrooms)
                => new(
                    "property_registry.home.register.bedrooms_negative",
                    new Dictionary<string, object?>
                    {
                        ["bedrooms"] = bedrooms,
                    });

            public static BusinessRuleViolationException BathroomsNonPositive(decimal bathrooms)
                => new(
                    "property_registry.home.register.bathrooms_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["bathrooms"] = bathrooms,
                    });

            public static BusinessRuleViolationException SquareFootageNonPositive(decimal squareFootage)
                => new(
                    "property_registry.home.register.square_footage_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["squareFootage"] = squareFootage,
                    });

            public static BusinessRuleViolationException YearBuiltOutOfRange(int yearBuilt, int minYear, int maxYear)
                => new(
                    "property_registry.home.register.year_built_out_of_range",
                    new Dictionary<string, object?>
                    {
                        ["yearBuilt"] = yearBuilt,
                        ["minYear"] = minYear,
                        ["maxYear"] = maxYear,
                    });
        }

        public static class IncomeLimit
        {
            public static BusinessRuleViolationException PercentOutOfRange(int percentOfAmi, int minimum, int maximum)
                => new(
                    "property_registry.home.income_limit.percent_out_of_range",
                    new Dictionary<string, object?>
                    {
                        ["percentOfAmi"] = percentOfAmi,
                        ["minimum"] = minimum,
                        ["maximum"] = maximum,
                    });
        }

        public static class ReleaseForSale
        {
            public static BusinessRuleViolationException NotHeld()
                => new(
                    "property_registry.home.release_for_sale.not_held");
        }

        public static class RecordAssessment
        {
            public static BusinessRuleViolationException AssessedValueNegative(decimal assessedValue)
                => new(
                    "property_registry.home.record_assessment.assessed_value_negative",
                    new Dictionary<string, object?>
                    {
                        ["assessedValue"] = assessedValue,
                    });
        }
    }

    public static class UnitAssessment
    {
        public static class Create
        {
            public static BusinessRuleViolationException AssessedValueNegative(decimal assessedValue)
                => new(
                    "property_registry.unit_assessment.create.assessed_value_negative",
                    new Dictionary<string, object?>
                    {
                        ["assessedValue"] = assessedValue,
                    });

            public static BusinessRuleViolationException AssessmentDateInFuture(DateOnly assessmentDate, DateOnly today)
                => new(
                    "property_registry.unit_assessment.create.assessment_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["assessmentDate"] = assessmentDate,
                        ["today"] = today,
                    });
        }
    }

    public static class DevelopmentProject
    {
        public static class Initiate
        {
            public static BusinessRuleViolationException NameRequired()
                => new(
                    "property_registry.development_project.initiate.name_required");

            public static BusinessRuleViolationException DescriptionRequired()
                => new(
                    "property_registry.development_project.initiate.description_required");
        }

        public static class InitiateRehabilitation
        {
            public static BusinessRuleViolationException HomeIdsRequired()
                => new(
                    "property_registry.development_project.initiate_rehabilitation.home_ids_required");
        }
    }
}
