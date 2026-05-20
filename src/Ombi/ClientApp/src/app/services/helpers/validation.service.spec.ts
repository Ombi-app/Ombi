import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ValidationService } from './validation.service';
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';

describe('ValidationService', () => {
  let service: ValidationService;
  let form: UntypedFormGroup;

  beforeEach(() => {
    service = new ValidationService();
    form = new UntypedFormGroup({
      name: new UntypedFormControl('', [Validators.required]),
      email: new UntypedFormControl(''),
    });
  });

  describe('disableValidation', () => {
    it('should clear validators on the named control', () => {
      // name starts with required validator, so empty value is invalid
      expect(form.controls['name'].valid).toBe(false);

      service.disableValidation(form, 'name');

      // After clearing, the empty value should be valid
      expect(form.controls['name'].valid).toBe(true);
      expect(form.controls['name'].validator).toBeNull();
    });

    it('should update validity after clearing', () => {
      form.controls['name'].setValue('');
      service.disableValidation(form, 'name');
      expect(form.controls['name'].errors).toBeNull();
    });
  });

  describe('enableValidation', () => {
    it('should set required validator by default', () => {
      // email starts with no validators
      form.controls['email'].setValue('');
      expect(form.controls['email'].valid).toBe(true);

      service.enableValidation(form, 'email');

      // Now it should be invalid because it's empty and required
      expect(form.controls['email'].valid).toBe(false);
      expect(form.controls['email'].errors).toEqual({ required: true });
    });

    it('should set custom validators when provided', () => {
      service.enableValidation(form, 'email', [Validators.email]);

      form.controls['email'].setValue('not-an-email');
      expect(form.controls['email'].valid).toBe(false);

      form.controls['email'].setValue('user@example.com');
      expect(form.controls['email'].valid).toBe(true);
    });

    it('should make a control valid when value satisfies the required validator', () => {
      service.enableValidation(form, 'email');
      form.controls['email'].setValue('hello');
      expect(form.controls['email'].valid).toBe(true);
    });
  });
});
