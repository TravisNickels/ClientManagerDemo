export interface Phone {
  id: string
  clientId: string
  phoneNumber: string
  phoneType: string
}

export type CreatePhoneRequest = Omit<Phone, 'id' | 'clientId'>
export type UpdatePhoneRequest = Phone
