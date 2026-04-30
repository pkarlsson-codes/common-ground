export interface RecordHomeAssessmentRequest {
  assessedValue: number
  assessmentDate: string
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

export interface InitiateDevelopmentProjectRequest {
  landParcelId: string
  name: string
  description?: string
  projectedStartDate?: string
  projectedEndDate?: string
  budget?: number
  homeIds?: string[]
}

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
  groundLeaseId: string
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
