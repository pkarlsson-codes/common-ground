using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.Documents.Application.Errors;

public static class BusinessRuleViolation
{
    public static class Document
    {
        public static class Upload
        {
            public static BusinessRuleViolationException SizeNonPositive(long sizeBytes)
                => new(
                    "documents.document.upload.size_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["sizeBytes"] = sizeBytes,
                    });
        }

        public static class Attach
        {
            public static BusinessRuleViolationException DocumentDeleted()
                => new(
                    "documents.document.attach.document_deleted");
        }

        public static class SoftDelete
        {
            public static BusinessRuleViolationException LegalHoldActive()
                => new(
                    "documents.document.soft_delete.legal_hold_active");
        }

        public static class AddVersion
        {
            public static BusinessRuleViolationException SizeNonPositive(long sizeBytes)
                => new(
                    "documents.document.add_version.size_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["sizeBytes"] = sizeBytes,
                    });

            public static BusinessRuleViolationException DocumentDeleted()
                => new(
                    "documents.document.add_version.document_deleted");
        }

        public static class UpdateMetadata
        {
            public static BusinessRuleViolationException DocumentDeleted()
                => new(
                    "documents.document.update_metadata.document_deleted");

            public static BusinessRuleViolationException TitleBlank()
                => new(
                    "documents.document.update_metadata.title_blank");
        }

        public static class Tag
        {
            public static BusinessRuleViolationException DocumentDeleted()
                => new(
                    "documents.document.tag.document_deleted");
        }
    }
}