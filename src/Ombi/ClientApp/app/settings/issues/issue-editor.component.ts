import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { IIssueCategory } from "../../interfaces";
import { IssuesService, NotificationService } from "../../services";

@Component({
    selector: "ngbd-modal-content",
    templateUrl: "./issue-editor.component.html",
})
export class IssueEditorComponent implements OnInit {

    @Input() public issueCategory: IIssueCategory;

    @Output() public onSuccessfulEdit = new EventEmitter<IIssueCategory>();

    public title: string;
    public submitFormButtonText: string;
    public form: FormGroup;

    private isEditMode: boolean;

    constructor(public activeModal: NgbActiveModal,
                private issuesService: IssuesService,
                private notificationService: NotificationService,
                private formBuilder: FormBuilder) {
    }

    public ngOnInit(): void {
        this.isEditMode = !!this.issueCategory.name;
        this.title = this.isEditMode ? "Edit Category" : "Add New Category";
        this.submitFormButtonText = this.isEditMode ? "Update" : "Add";
        this.form = this.formBuilder.group({
            name: [this.issueCategory.name, Validators.required],
            subjectPlaceholder: [this.issueCategory.subjectPlaceholder],
            descriptionPlaceholder: [this.issueCategory.descriptionPlaceholder],
        });
    }

    public onSubmit(form: FormGroup) {
        const categoryWithEnteredValues = {
            id: this.issueCategory.id,
            name: form.value.name,
            subjectPlaceholder: form.value.subjectPlaceholder,
            descriptionPlaceholder: form.value.descriptionPlaceholder,
        };

        if (this.isEditMode) {
            this.issuesService.updateCategory(categoryWithEnteredValues)
                .subscribe(
                    result => {
                        this.onSuccessfulEdit.emit(result);
                        this.activeModal.close();
                    },
                    error => {
                        this.notificationService.error("Unable to update this category.");
                    });
        } else {
            this.issuesService.createCategory(categoryWithEnteredValues)
                .subscribe(
                    result => {
                        this.onSuccessfulEdit.emit(result);
                        this.activeModal.close();
                    },
                    error => {
                        this.notificationService.error("Unable to create this category.");
                    });
        }
    }

}
