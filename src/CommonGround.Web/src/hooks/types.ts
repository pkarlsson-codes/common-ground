export interface RegisterBuildingRequest {
  landParcelId: string
  buildingType: string
  bedrooms: number
  bathrooms: number
  squareFootage: number
  yearBuilt: number
  initialAssessedValue: number
}

export interface RecordBuildingAssessmentRequest {
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

export const BUILDING_STATUSES = ['Available', 'Occupied', 'UnderRenovation'] as const
export const BUILDING_TYPES = ['SingleFamily', 'CondoUnit', 'Duplex', 'Townhouse'] as const
export const BUILDING_TYPE_LABELS: Record<string, string> = {
  SingleFamily: 'Single Family',
  CondoUnit: 'Condo Unit',
  Duplex: 'Duplex',
  Townhouse: 'Townhouse',
}
export const PARCEL_STATUSES = ['Available', 'Occupied', 'UnderDevelopment'] as const
export const ACQUISITION_METHODS = ['Purchase', 'Donation', 'Transfer'] as const

export const statusLabel = (s: string) => s.replace(/([A-Z])/g, ' $1').trim()
