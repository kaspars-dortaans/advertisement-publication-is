import type { IImageDto } from '@/services/api-client'
export interface IImageUploadDto extends IImageDto {
  file?: File
}
