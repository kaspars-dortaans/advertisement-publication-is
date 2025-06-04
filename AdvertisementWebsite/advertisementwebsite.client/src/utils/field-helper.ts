import { RequestExceptionResponse } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { useToast } from 'primevue'
import type {
  BaseFieldProps,
  FormContext,
  FormMeta,
  GenericObject,
  InputBindsConfig,
  LazyInputBindsConfig,
  Path,
  PathValue
} from 'vee-validate'
import { computed, type ComputedRef, type Ref } from 'vue'

//vee validate not exported generic types
export type TPath<TValues> = Path<TValues>
type TValue<TValues, Path extends TPath<TValues>> = PathValue<TValues, Path>
type TExtras = GenericObject
type Attributes = Ref<BaseFieldProps & TExtras>
type FieldConfig<TValues, Path extends TPath<TValues>> =
  | Partial<InputBindsConfig<TValue<TValues, Path>, TExtras>>
  | LazyInputBindsConfig<TValue<TValues, Path>, TExtras>

//FormContext property and function types
type DefineFieldFn<TValues extends GenericObject> = FormContext<TValues>['defineField']
type ErrorsProperty<TValues extends GenericObject> = FormContext<TValues>['errors']
type SetFieldErrorFn<TValues extends GenericObject> = FormContext<TValues>['setFieldError']
type SetErrorsFn<TValues extends GenericObject> = FormContext<TValues>['setErrors']

//FieldHelper types
export type Fields<TValues extends GenericObject> = Partial<{
  [key in TPath<TValues>]: Field<TValue<TValues, key>, TValues>
}>

//exported classes
export class Field<FieldType, TValues extends GenericObject> {
  private _path: TPath<TValues>
  private _errors: ErrorsProperty<TValues>
  private _setFieldError: SetFieldErrorFn<TValues>
  model: Ref<FieldType>
  attributes: Attributes

  get value() {
    return this.model.value
  }

  set value(newValue: FieldType) {
    this.model.value = newValue
  }

  get error() {
    return this._errors.value[this._path]
  }

  get hasError() {
    return !!this._errors.value[this._path]
  }

  get path() {
    return this._path
  }

  setErrors = (e: string | string[]) => {
    this._setFieldError(this._path, e)
  }

  clearErrors = () => {
    this._setFieldError(this._path, undefined)
  }

  constructor(
    path: TPath<TValues>,
    errors: ErrorsProperty<TValues>,
    model: Ref<FieldType>,
    attributes: Attributes,
    setFieldError: SetFieldErrorFn<TValues>
  ) {
    this.model = model
    this.attributes = attributes
    this._path = path
    this._errors = errors
    this._setFieldError = setFieldError
  }
}

export class FieldHelper<TValues extends GenericObject> {
  protected _defineField: DefineFieldFn<TValues>
  protected _setFieldError: SetFieldErrorFn<TValues>
  protected _setErrors: SetErrorsFn<TValues>
  protected _isFieldDirty: (path: TPath<TValues>) => boolean
  protected _errors: ErrorsProperty<TValues>
  protected _meta: ComputedRef<FormMeta<TValues>>
  protected _toast = useToast()
  protected _ls = LocaleService.get()

  fields: Fields<TValues> = {}

  readonly formErrorKey: TPath<TValues> = 'formLevelErrors' as TPath<TValues>

  //Functions and properties of FormContext are destructured in order to preserve their reactivity
  //For more info see https://vee-validate.logaretm.com/v4/guide/composition-api/caveats/#destructing-composable
  constructor(formContext: FormContext<TValues>) {
    const { errors, defineField, setFieldError, setErrors, isFieldDirty, meta } = formContext
    this._defineField = defineField
    this._setFieldError = setFieldError
    this._setErrors = setErrors
    this._isFieldDirty = isFieldDirty
    this._meta = meta
    this._errors = errors
  }

  formErrors = computed(() => {
    return this._errors.value[this.formErrorKey]
  })

  hasFormErrors = computed(() => {
    return !!this._errors.value[this.formErrorKey]
  })

  valuesChanged = computed(() => {
    return (
      this._meta.value.dirty &&
      Object.entries<Field<unknown, TValues>>(this.fields as GenericObject).some(
        (e) =>
          this._isFieldDirty(e[0] as TPath<TValues>) &&
          !this._compareFieldValue(
            e[1].model.value,
            this._meta.value.initialValues?.[e[0] as TPath<TValues>]
          )
      )
    )
  })

  private _compareFieldValue = (newValue: unknown, oldValue: unknown) => {
    if (newValue === oldValue) {
      return true
    }

    let newComparableItems = newValue
    let oldComparableItems = oldValue

    if (newValue && oldValue && typeof newValue === 'object' && typeof oldValue === 'object') {
      newComparableItems = Object.entries(newValue)
        .filter((e) => typeof e[1] !== 'function')
        .map((e) => e[1])
      oldComparableItems = Object.entries(oldValue)
        .filter((e) => typeof e[1] !== 'function')
        .map((e) => e[1])
    }

    if (Array.isArray(newComparableItems) && Array.isArray(oldComparableItems)) {
      if (newComparableItems.length !== oldComparableItems.length) {
        return false
      }

      for (let i = 0; i < newComparableItems.length; i++) {
        if (!this._compareFieldValue(newComparableItems[i], oldComparableItems[i])) {
          return false
        }
      }

      //All items where equal
      return true
    }

    return false
  }

  defineField = (path: Path<TValues>, config?: FieldConfig<TValues, Path<TValues>>) => {
    const [field, fieldAttrs] = this._defineField(path, config)
    this.fields[path] = new Field(path, this._errors, field, fieldAttrs, this._setFieldError)
  }

  /** Define multiple fields, with possible parameter values in format { "fieldName1", ["fieldName2", configObject] } */
  defineMultipleFields = (
    fields: (Path<TValues> | [Path<TValues>, FieldConfig<TValues, Path<TValues>>?])[]
  ) => {
    for (const field of fields) {
      if (Array.isArray(field)) {
        this.defineField(field[0], field[1])
      } else {
        this.defineField(field)
      }
    }
    return this.fields
  }

  handleErrors = (errorObj: unknown) => {
    if (!(errorObj instanceof RequestExceptionResponse)) {
      this._toast.add({
        summary: this._ls.l('errors.errorOccurred'),
        detail:
          typeof errorObj === 'object' && errorObj && 'message' in errorObj && errorObj.message
            ? errorObj.message
            : '',
        severity: 'error',
        group: 'tr'
      })
      return
    }

    //Handle field validation errors
    if (errorObj.errors) {
      for (const errorKey in errorObj.errors) {
        //Replace surrounding brackets with dot for array element validation errors
        let fieldKey = errorKey
        if (errorKey.includes('[')) {
          fieldKey = fieldKey.replace(/\[([^[\]]*)\]/g, '.$1')
        }

        if (fieldKey in this.fields) {
          this._setFieldError(fieldKey as Path<TValues>, errorObj.errors[fieldKey])
        }
      }
    }

    //Handle form errors
    if (errorObj.errorCodes?.length) {
      const formErrorObject: Partial<{ [key in TPath<TValues>]: string }> = {}
      formErrorObject[this.formErrorKey as Path<TValues>] = errorObj.errorCodes[0]
      this._setErrors(formErrorObject)
    }
  }

  clearErrors = () => {
    const errorKeys = Object.keys(this._errors.value) as Path<TValues>[]
    const errObj: Partial<{ [key in TPath<TValues>]: undefined }> = {}
    for (const errorKey of errorKeys) {
      errObj[errorKey] = undefined
    }
    this._setErrors(errObj)
  }
}
