/** Mirrors FinancialInteropController DTOs (JSON camelCase). */
export type PatientCoverageRegistrationDto = {
  id: string
  patientId: string
  memberIdentifier: string
  payorDisplayName: string
  planDisplayName: string
  periodStart: string
  periodEnd: string | null
  fhirCoverageResourceId: string | null
  createdAtUtc: string
}

export type EligibilityInquiryDto = {
  id: string
  patientCoverageRegistrationId: string
  patientId: string
  status: string
  outcomeCode: string
  notes: string | null
  createdAtUtc: string
}

export type FinancialClaimDto = {
  id: string
  treatmentSessionId: string
  patientId: string
  patientCoverageRegistrationId: string
  fhirEncounterReference: string | null
  claimUse: string
  status: string
  externalClaimId: string | null
  externalClaimResponseId: string | null
  outcomeDisplay: string | null
  createdAtUtc: string
  updatedAtUtc: string
}

export type ExplanationOfBenefitDto = {
  id: string
  dialysisFinancialClaimId: string
  treatmentSessionId: string
  fhirExplanationOfBenefitReference: string
  patientResponsibilityAmount: number | null
  linkedAtUtc: string
  createdAtUtc: string
}

export type FinancialSessionTimelineDto = {
  treatmentSessionId: string
  resolvedPatientId: string | null
  coverages: PatientCoverageRegistrationDto[]
  eligibilityInquiries: EligibilityInquiryDto[]
  claims: FinancialClaimDto[]
  explanationOfBenefits: ExplanationOfBenefitDto[]
}
