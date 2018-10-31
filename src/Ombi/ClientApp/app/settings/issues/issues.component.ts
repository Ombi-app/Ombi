import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";

import { IIssueCategory } from "../../interfaces";
import { IssuesService, NotificationService, SettingsService } from "../../services";
import { IssueEditorComponent } from "./issue-editor.component";

@Component({
    templateUrl: "./issues.component.html",
})
export class IssuesComponent implements OnInit {

    public categories: IIssueCategory[];
    public form: FormGroup;

    constructor(private issuesService: IssuesService,
                private settingsService: SettingsService,
                private readonly fb: FormBuilder,
                private notificationService: NotificationService,
                private modalService: NgbModal) {  }

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
        const modalRef = this.modalService.open(IssueEditorComponent, 
            { container:"ombi", backdropClass:"custom-modal-backdrop", windowClass:"window" });
        const modalComponent = <IssueEditorComponent> modalRef.componentInstance;
        modalComponent.issueCategory = {
            id: 0,
            name: "",
            subjectPlaceholder: "",
            descriptionPlaceholder: "",
        };
        modalComponent.onSuccessfulEdit.subscribe(
            (newCategory: IIssueCategory) => { this.categories.push(newCategory); });
    }

    public editCategory(categoryToEdit: IIssueCategory): void {
        console.log(categoryToEdit);
        const modalRef = this.modalService.open(IssueEditorComponent, 
            { container: "ombi", backdropClass: "custom-modal-backdrop", windowClass: "window" });
        const modalComponent = <IssueEditorComponent> modalRef.componentInstance;
        modalComponent.issueCategory = categoryToEdit;

        modalComponent.onSuccessfulEdit.subscribe(
            (updatedCategory: IIssueCategory) => {
                this.categories = this.categories.map(c => {
                    return c.id === updatedCategory.id ? updatedCategory : c;
                });
            });
    }

    public deleteCategory(id: number) {
        this.issuesService.deleteCategory(id).subscribe(x => {
            if (x) {
                this.categories = this.categories.filter(c => {
                    return c.id === id;
                });
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
