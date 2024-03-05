import { Component, OnInit, SecurityContext } from "@angular/core";
import { UntypedFormBuilder, UntypedFormGroup, Validators } from "@angular/forms";
import { DomSanitizer } from "@angular/platform-browser";
import { AuthService } from "../auth/auth.service";
import { CustomPageService, NotificationService } from "../services";

@Component({
    templateUrl: "./custompage.component.html",
}) 
export class CustomPageComponent implements OnInit {

    public form: UntypedFormGroup;
    public isEditing: boolean;
    public isAdmin: boolean;

    constructor(private auth: AuthService, private settings: CustomPageService, private fb: UntypedFormBuilder,
                private notificationService: NotificationService, 
                private sanitizer: DomSanitizer) {
    }

    public ngOnInit() {
        this.settings.getCustomPage().subscribe(x => {
            x.html = this.sanitizer.sanitize(SecurityContext.HTML, this.sanitizer.bypassSecurityTrustHtml(x.html));
            this.form = this.fb.group({
                enabled: [x.enabled],
                title: [x.title, [Validators.required]],
                html: [x.html, [Validators.required]],
                fontAwesomeIcon: [x.fontAwesomeIcon, [Validators.required]],
            });
        });
        this.isAdmin = this.auth.hasRole("EditCustomPage");
    }

    public onSubmit() {
        if (this.form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }
        this.settings.saveCustomPage(this.form.value).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Custom Page settings");
            } else {
                this.notificationService.success("There was an error when saving the Custom Page settings");
            }
        });
    }
}
