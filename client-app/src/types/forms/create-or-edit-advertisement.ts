import type { CreateOrEditAdvertisementRequest } from '@/services/api-client'
import type { IImageUploadDto } from '../image/image-upload-dto'

export type CreateOrEditAdvertisementForm = Omit<
  CreateOrEditAdvertisementRequest,
  'attributeValues' | 'imagesToAdd'
> & {
  attributeValues: (string | number)[]
  imagesToAdd: IImageUploadDto[]
}
