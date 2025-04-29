import { LocaleService } from '@/services/locale-service'

const ls = LocaleService.get()

export const getPageReportTemplate = () => {
  return ls.l('dataTable.pageReportTemplate', '{first}', '{last}', '{totalRecords}')
}
