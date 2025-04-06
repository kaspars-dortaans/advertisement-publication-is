import type { IFileHashDto } from '@/types/image/file-hash'

export interface IImageUploadDto extends IFileHashDto {
  id?: number
  url: string
}
