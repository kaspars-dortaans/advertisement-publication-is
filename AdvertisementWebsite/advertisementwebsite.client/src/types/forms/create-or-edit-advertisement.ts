import type {
  CreateOrEditAdvertisementRequest,
  Int32StringKeyValuePair
} from '@/services/api-client'
import type { IImageUploadDto } from '../image/image-upload-dto'

export type CreateOrEditAdvertisementForm = Omit<
  CreateOrEditAdvertisementRequest,
  'attributeValues' | 'imagesToAdd' | 'ownerId'
> & {
  attributeValues: (string | number | undefined)[]
  imagesToAdd: IImageUploadDto[]
  ownerId: Int32StringKeyValuePair
}
