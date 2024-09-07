import { Injectable } from "@angular/core";
import { UntypedFormGroup, ValidatorFn, Validators } from "@angular/forms";

@Injectable()
export class ValidationService {

    /**
     * Disable validation on a control
     * @param form
     * @param name
     */
    public disableValidation(form: UntypedFormGroup, name: string) {
        form.controls[name].clearValidators();
        form.controls[name].updateValueAndValidity();
    }

    /**
     * Enable validation with the default validation attribute of required
     * @param form
     * @param name
     */
    public enableValidation(form: UntypedFormGroup, name: string): void;
    public enableValidation(form: UntypedFormGroup, name: string, validators?: ValidatorFn[]) {
        if (validators) {
            // If we provide some use them
            form.controls[name].setValidators(validators);
        } else {
            // It's just required by default
            form.controls[name].setValidators([Validators.required]);
        }
        form.controls[name].updateValueAndValidity();
    }

}
