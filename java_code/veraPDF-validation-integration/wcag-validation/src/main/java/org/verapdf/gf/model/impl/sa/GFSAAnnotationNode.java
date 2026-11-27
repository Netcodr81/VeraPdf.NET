/*
 * This file is part of veraPDF WCAG Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF WCAG Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF WCAG Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF WCAG Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.sa;

import org.verapdf.as.ASAtom;
import org.verapdf.cos.COSArray;
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.pd.PDAnnotation;
import org.verapdf.pd.PDPage;
import org.verapdf.pd.actions.PDAction;
import org.verapdf.tools.StaticResources;
import org.verapdf.wcag.algorithms.entities.AnnotationNode;
import org.verapdf.wcag.algorithms.entities.geometry.BoundingBox;

import java.util.Objects;

/**
 * Converts a PDF annotation into the WCAG algorithms' spatial semantic node.
 *
 * <p>The node carries the annotation subtype, transformed rectangle, destination page, and
 * structure-element reference number. Page association can come from the annotation's page key
 * or from scanning page annotation arrays; destinations may be direct or represented by a GoTo
 * action. The WCAG algorithms consume this node when associating annotations with tagged
 * content.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type is
 * {@code VeraPdf.Validation.Wcag.Model.Impl.Sa.GFSAAnnotationNode}; it adapts the not-yet-ported
 * {@code org.verapdf.wcag.algorithms.entities.AnnotationNode}. Use
 * {@code VeraPdf.Parser.Pd.PdfAnnotation}, {@code PdfPage}, and COS references, and preserve the
 * transformation and null-page behavior when porting the page scans.</p>
 */
public class GFSAAnnotationNode extends AnnotationNode {

	public GFSAAnnotationNode(PDAnnotation annotation) {
		super(annotation.getSubtype().getValue(), getBoundingBox(annotation),
				getPageNumber(getDestination(annotation, ASAtom.D)),
				getObjectKeyNumber(getDestination(annotation, ASAtom.SD)));
	}

	/** Returns the annotation rectangle in the page's current display coordinate system. */
	public static BoundingBox getBoundingBox(PDAnnotation annotation) {
		Integer pageNumber = getAnnotationPageNumber(annotation);
		double[] rect = annotation.getRect();
		BoundingBox boundingBox = rect == null ? new BoundingBox() : new BoundingBox(pageNumber, rect);
		if (pageNumber != null) {
			PDPage page = StaticResources.getDocument().getPages().get(pageNumber);
			boundingBox = GFSAPage.createCurrentTransformationMatrix(page).transformBoundingBox(boundingBox);
		}
		return boundingBox;
	}

	/** Finds the owning page from the annotation's explicit page reference or page arrays. */
	private static Integer getAnnotationPageNumber(PDAnnotation annotation) {
		Integer pageNumber = getPageNumber(annotation.getKey(ASAtom.P));
		if (pageNumber != null) {
			return pageNumber;
		}
		if (annotation.getObject().getKey() == null) {
			return null;
		}
		for (PDPage page : StaticResources.getDocument().getPages()) {
			COSObject annots = page.getKey(ASAtom.ANNOTS);
			if (!annots.empty() && annots.getType() == COSObjType.COS_ARRAY) {
				for (COSObject annot : (COSArray) annots.getDirectBase()) {
					if (annotation.getObject().getKey().equals(annot.getKey())) {
						return page.getPageNumber();
					}
				}
			}
		}
		return null;
	}

	/** Resolves a destination only when it names a page object in this document. */
	private static Integer getPageNumber(COSObject obj) {
		if (obj != null && ASAtom.PAGE == obj.getNameKey(ASAtom.TYPE)) {
			for (PDPage page : StaticResources.getDocument().getPages()) {
				if (Objects.equals(obj.getKey(), page.getObject().getKey())) {
					return page.getPageNumber();
				}
			}
		}
		return null;
	}

	/** Returns a referenced structure element's object number, or null for other targets. */
	private static Integer getObjectKeyNumber(COSObject obj) {
		if (obj != null && ASAtom.STRUCT_ELEM == obj.getNameKey(ASAtom.TYPE)) {
			return obj.getKey() != null ? obj.getKey().getNumber() : null;
		}
		return null;
	}

	/** Reads a direct destination or falls back to the destination on a GoTo action. */
	private static COSObject getDestination(PDAnnotation annot, ASAtom key) {
		COSObject destination = annot.getDestination();
		if (destination == null || destination.empty()) {
			PDAction action = annot.getA();
			if (action != null && ASAtom.GO_TO == action.getSubtype()) {
				destination = action.getObject().getKey(key);
			}
		}
		if (destination == null || destination.empty()) {
			return null;
		}
		return PDAnnotation.getPageFromDestination(destination, key);
	}
}
