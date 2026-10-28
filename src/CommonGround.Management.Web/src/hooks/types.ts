export interface RecordHomeAssessmentRequest {
  assessedValue: number
  assessmentDate: string
  appraiserName?: string
  conditionNotes?: string
}

export interface RegisterLandParcelRequest {
  legalParcelNumber: string
  address: string
  areaAcres: number
  acquisitionDate: string
  acquisitionMethod: string
  covenantTerms?: string
}

export const BUILDING_TYPES = ['SingleFamily', 'CondoUnit', 'Duplex', 'Townhouse'] as const
export const BUILDING_TYPE_LABELS: Record<string, string> = {
  SingleFamily: 'Single Family',
  CondoUnit: 'Condo Unit',
  Duplex: 'Duplex',
  Townhouse: 'Townhouse',
}
export const HOME_TYPE_LABELS: Record<string, string> = {
  SingleFamily: 'Single Family',
  CondoUnit: 'Condo Unit',
  Duplex: 'Duplex',
  Townhouse: 'Townhouse',
}
export const HOME_STATUSES = ['Available', 'PendingOccupancy', 'Occupied', 'Resale'] as const
export const DEVELOPMENT_PROJECT_STATUSES = ['Initiated', 'Active', 'Completed', 'Abandoned'] as const
export const PROJECT_TYPE_LABELS: Record<string, string> = {
  NewConstruction: 'New Construction',
  Rehabilitation: 'Rehabilitation',
}
export const ACQUISITION_METHODS = ['Purchase', 'Donation', 'Transfer'] as const

export const statusLabel = (s: string) => s.replace(/([A-Z])/g, ' $1').trim()

// ── Homeownership ──────────────────────────────────────────────────────────

export const GROUND_LEASE_STATUSES = ['Active', 'Inactive'] as const
export const FORMULA_KINDS = ['AppraisalBased', 'IndexBased', 'FixedPercentage'] as const
export const FORMULA_KIND_LABELS: Record<string, string> = {
  AppraisalBased:  'Appraisal Based',
  IndexBased:      'Index Based',
  FixedPercentage: 'Fixed Percentage',
}
export const INDEX_TYPES = ['CPI', 'AMI', 'Custom'] as const

export const WORKFLOW_PHASES = [
  'Initiated', 'InProgress', 'ClosingScheduled', 'Closed', 'Failed', 'Withdrawn', 'Stalled',
] as const

export interface CreateGroundLeaseForm {
  landParcelId: string
  homeId: string
  leaseStartDate: string
  termYears: number
  covenantTerms: string
  formulaKind: string
  appreciationShare: number
  indexType: string
  baselineIndexValue: number
  annualPercentage: number
  improvementsCap: number
  hasImprovementsCap: boolean
}

export interface OnboardHomeownerForm {
  fullName: string
  email: string
  phone: string
}

export interface RecordCheckInForm {
  checkInDate: string
  notes: string
}

export interface ResaleEstimateForm {
  originalPurchasePrice: number
  currentAppraisalValue: number
  originalAppraisalValue: number
  ownershipYears: number
  approvedImprovements: number
  currentIndexValue: number
}

export interface InitiateWorkflowForm {
  groundLeaseTemplateId: string
  landParcelId: string
  homeId: string
  buyerFullName: string
  buyerEmail: string
  buyerPhone: string
  purchasePrice: number
}

export interface RecordEligibilityForm {
  verifiedDate: string
  notes: string
}

export interface RecordFinancingForm {
  lenderName: string
  committedAmount: number
  preapprovalLetterDate: string
}

export interface RecordInspectionForm {
  inspectionDate: string
  inspectorName: string
  notes: string
}

export interface RecordTitleForm {
  clearedDate: string
  titleCompanyName: string
}

export interface RecordLeaseExecutedForm {
  signedDate: string
}

export interface ScheduleClosingForm {
  scheduledDate: string
}

export interface RecordClosedForm {
  actualClosingDate: string
  closingNotes: string
}

export interface FailWorkflowForm {
  reason: string
}

export interface WithdrawWorkflowForm {
  reason: string
}

export interface StallWorkflowForm {
  reason: string
  resolutionTargetDate: string
}

export interface CreateGroundLeaseTemplateForm {
  templateName: string
  termYears: number
  covenantTerms: string
  formulaKind: string
  appreciationShare: number | null
  indexType: string | null
  baselineIndexValue: number | null
  annualPercentage: number | null
  improvementsCap: number | null
  hasImprovementsCap: boolean
}

// ── Stewardship ────────────────────────────────────────────────────────────

export const LEASE_VIOLATION_TYPE_LABELS: Record<string, string> = {
  Occupancy:             'Occupancy',
  UnpermittedAlteration: 'Unpermitted Alteration',
  PhysicalCondition:     'Physical Condition',
  LeaseTermViolation:    'Lease Term Violation',
}
export const COUNSELING_OUTCOME_LABELS: Record<string, string> = {
  CounselingCompleted:   'Counseling Completed',
  HomeownerUnresponsive: 'Homeowner Unresponsive',
  NotEligible:           'Not Eligible',
}
export const FORBEARANCE_OUTCOMES = ['Granted', 'Denied'] as const
export const FORECLOSURE_PREVENTION_OUTCOME_LABELS: Record<string, string> = {
  InterventionSuccessful: 'Intervention Successful',
  HomeownerSold:          'Homeowner Sold',
  CurePeriodExercised:    'Cure Period Exercised',
  LeaseTerminated:        'Lease Terminated',
}
