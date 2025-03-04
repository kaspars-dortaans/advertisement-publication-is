import { RequestExceptionResponse } from '@/services/api-client'
import type {
  BaseFieldProps,
  FormContext,
  GenericObject,
  InputBindsConfig,
  LazyInputBindsConfig,
  Path,
  PathValue
} from 'vee-validate'
import { computed, type Ref } from 'vue'

//vee validate not exported generic types
type TPath<TValues> = Path<TValues>
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
type Fields<TValues extends GenericObject> = Partial<{
  [key in TPath<TValues>]: Field<TValue<TValues, key>, TValues>
}>

//exported classes
export class Field<FieldType, TValues extends GenericObject> {
  private _path: TPath<TValues>
  private _errors: ErrorsProperty<TValues>
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

  constructor(
    path: TPath<TValues>,
    errors: ErrorsProperty<TValues>,
    model: Ref<FieldType>,
    attributes: Attributes
  ) {
    this.model = model
    this.attributes = attributes
    this._path = path
    this._errors = errors
  }
}

export class FieldHelper<TValues extends GenericObject> {
  protected _defineField: DefineFieldFn<TValues>
  protected _setFieldError: SetFieldErrorFn<TValues>
  protected _setErrors: SetErrorsFn<TValues>
  protected _errors: ErrorsProperty<TValues>

  fields: Fields<TValues> = {}

  readonly formErrorKey: TPath<TValues> = 'formLevelErrors' as TPath<TValues>

  //Functions and proprties of FormContext are destructured in order to preserve their reactivity
  //For more info see https://vee-validate.logaretm.com/v4/guide/composition-api/caveats/#destructing-composable
  constructor(formContext: FormContext<TValues>) {
    const { errors, defineField, setFieldError, setErrors } = formContext
    this._defineField = defineField
    this._setFieldError = setFieldError
    this._setErrors = setErrors
    this._errors = errors
  }

  formErrors = computed(() => {
    return this._errors.value[this.formErrorKey]
  })

  hasFormErrors = computed(() => {
    return !!this._errors.value[this.formErrorKey]
  })

  defineField = (path: Path<TValues>, config?: FieldConfig<TValues, Path<TValues>>) => {
    const [field, fieldAttrs] = this._defineField(path, config)
    this.fields[path] = new Field(path, this._errors, field, fieldAttrs)
  }

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
      throw errorObj
    }

    //Handle field validation errors
    if (errorObj.errors) {
      for (const fieldKey in errorObj.errors) {
        //convert first char to lowercase for field names to match api-client dto objects field names
        const jsFieldKey = fieldKey[0].toLocaleLowerCase() + fieldKey.slice(1)
        if (jsFieldKey in this.fields) {
          this._setFieldError(jsFieldKey as Path<TValues>, errorObj.errors[fieldKey])
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
