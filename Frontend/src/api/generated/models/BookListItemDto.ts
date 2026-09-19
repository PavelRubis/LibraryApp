/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AuthorDto } from './AuthorDto';
export type BookListItemDto = {
    id?: string;
    title?: string | null;
    publicationYear?: number;
    authors?: Array<AuthorDto> | null;
    createdAt?: string;
    updatedAt?: string | null;
    rowVersion?: string | null;
};

