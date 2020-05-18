import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";

import { IIssueCategory } from "../../interfaces";
import { IssuesService, NotificationService, SettingsService } from "../../services";

@Component({
    templateUrl: "./issues.component.html",
    styleUrls: ["./issues.component.scss"]
})
export class IssuesComponent implements OnInit {

    public categories: IIssueCategory[];
    public categoryToAdd: IIssueCategory = {id: 0, value: ""};
    public form: FormGroup;

    constructor(private issuesService: IssuesService,
                private settingsService: SettingsService,
                private readonly fb: FormBuilder,
                private notificationService: NotificationService) {  }

    public ngOnInit() {
        this.settingsService.getIssueSettings().subscribe(x => {
            this.form = this.fb.group({
                enabled:                    [x.enabled],
                enableInProgress:           [x.enableInProgress],
                deleteIssues:               [x.deleteIssues],
                daysAfterResolvedToDelete:  [x.daysAfterResolvedToDelete],
            });
        });
        this.getCategories();
    }    

    public addCategory(): void {
        this.issuesService.createCategory(this.categoryToAdd).subscribe(x => {
            if(x) {
                this.getCategories();
                this.categoryToAdd.value = "";
            }
        });
    }

    public deleteCategory(id: number) {
        this.issuesService.deleteCategory(id).subscribe(x => {
            if(x) {
                this.getCategories();
            }
        });
    }
    
    public onSubmit(form: FormGroup) {
        if (form.invalid) {
            this.notificationService.error("Please check your entered values");
            return;
        }

        const settings = form.value;

        if(settings.deleteIssues && settings.daysAfterResolvedToDelete <= 0) {
            this.notificationService.error("You need to enter days greater than 0");
            return;
        }

        this.settingsService.saveIssueSettings(settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved the Issue settings");
            } else {
                this.notificationService.success("There was an error when saving the Issue settings");
            }
        });
    }

    private getCategories() {
        this.issuesService.getCategories().subscribe(x => {
            this.categories = x;
        });
    }
}
