/**
 * Validacijos funkcijos formoms
 */

/**
 * Tikrina ar el. paštas teisingas
 */
export function validateEmail(email: string | null | undefined): boolean {
  if (!email) return false
  
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return emailRegex.test(email)
}

/**
 * Tikrina ar laukas užpildytas
 */
export function validateRequired(value: any): boolean {
  if (value === null || value === undefined) return false
  if (typeof value === 'string') return value.trim() !== ''
  if (typeof value === 'number') return !isNaN(value)
  if (Array.isArray(value)) return value.length > 0
  return !!value
}

/**
 * Tikrina minimalų ilgį
 */
export function validateMinLength(value: string | null | undefined, minLength: number): boolean {
  if (!value) return false
  return value.length >= minLength
}

/**
 * Tikrina maksimalų ilgį
 */
export function validateMaxLength(value: string | null | undefined, maxLength: number): boolean {
  if (!value) return true // Jei tuščias, laikome teisingą (naudok validateRequired atskirai)
  return value.length <= maxLength
}

/**
 * Tikrina ar slaptažodis pakankamai stiprus
 * Bent 8 simboliai, bent 1 raidė ir 1 skaičius
 */
export function validatePassword(password: string | null | undefined): boolean {
  if (!password) return false
  
  const hasMinLength = password.length >= 8
  const hasLetter = /[a-zA-Z]/.test(password)
  const hasNumber = /\d/.test(password)
  
  return hasMinLength && hasLetter && hasNumber
}

/**
 * Tikrina ar slaptažodžiai sutampa
 */
export function validatePasswordMatch(password: string, confirmPassword: string): boolean {
  return password === confirmPassword
}

/**
 * Tikrina ar skaičius teigiamas
 */
export function validatePositiveNumber(value: number | null | undefined): boolean {
  if (value === null || value === undefined) return false
  return value > 0
}

/**
 * Tikrina ar skaičius yra intervale
 */
export function validateNumberRange(
  value: number | null | undefined,
  min: number,
  max: number
): boolean {
  if (value === null || value === undefined) return false
  return value >= min && value <= max
}

/**
 * Tikrina ar pabaigos data yra po pradžios datos
 */
export function validateDateRange(startDate: string | Date, endDate: string | Date): boolean {
  try {
    const start = new Date(startDate)
    const end = new Date(endDate)
    
    if (isNaN(start.getTime()) || isNaN(end.getTime())) return false
    
    return end >= start
  } catch {
    return false
  }
}

/**
 * Tikrina URL formatą
 */
export function validateUrl(url: string | null | undefined): boolean {
  if (!url) return false
  
  try {
    new URL(url)
    return true
  } catch {
    return false
  }
}

/**
 * Grąžina klaidos pranešimą pagal validacijos tipą
 */
export function getValidationMessage(
  field: string,
  validationType: 'required' | 'email' | 'minLength' | 'maxLength' | 'password' | 'positive',
  params?: any
): string {
  const messages: Record<string, string> = {
    required: `${field} yra privalomas`,
    email: 'Neteisingas el. pašto formatas',
    minLength: `${field} turi būti bent ${params} simbolių`,
    maxLength: `${field} negali būti ilgesnis nei ${params} simbolių`,
    password: 'Slaptažodis turi būti bent 8 simbolių ir turėti raidę bei skaičių',
    positive: `${field} turi būti teigiamas skaičius`
  }
  
  return messages[validationType] || 'Neteisingas laukas'
}

/**
 * Validacijos rezultato tipas
 */
export interface ValidationResult {
  valid: boolean
  message?: string
}

/**
 * Formos validatorius
 * Grąžina validacijos rezultatą su pranešimu
 */
export function validate(
  value: any,
  rules: Array<{
    validator: (val: any) => boolean
    message: string
  }>
): ValidationResult {
  for (const rule of rules) {
    if (!rule.validator(value)) {
      return {
        valid: false,
        message: rule.message
      }
    }
  }
  
  return { valid: true }
}
