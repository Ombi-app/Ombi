import { IIssueCategory, IUser, RequestType } from ".";

export interface IIssues {
    id?: number;
    title: string;
    requestType: RequestType;
    providerId: string;
    subject: string;
    description: string;
    issueCategory: IIssueCategory;
    issueCategoryId: number;
    status: IssueStatus;
    resolvedDate?: Date;
    comments: IIssueComments[];
    requestId: number | undefined;
    userReported: IUser | undefined;
}

export enum IssueStatus {
    Pending = 0,
    InProgress = 1,
    Resolved = 2,
}

export interface IIssueCount {
    pending: number;
    inProgress: number;
    resolved: number;
}

export interface IPagenator {
        first: number;
        rows: number;
        page: number;
        pageCount: number;
}

export interface IIssueComments {
    userId: string;
    comment: string;
    movieIssueId: number | undefined;
    tvIssueId: number | undefined;
    date: Date;
    user: IUser;
    issues: IIssues | undefined;
}

export interface IIssuesChat {
    id: number;
    comment: string;
    date: Date;
    username: string;
    adminComment: boolean;
}

export interface INewIssueComments {
    comment: string;
    issueId: number;
}

export interface IUpdateStatus {
    issueId: number;
    status: IssueStatus;
}
