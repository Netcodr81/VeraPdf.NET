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
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.gf.model.impl.containers.StaticStorages;
import org.verapdf.model.GenericModelObject;
import org.verapdf.model.salayer.SAAnnotation;
import org.verapdf.pd.*;
import org.verapdf.pd.structure.PDNumberTreeNode;
import org.verapdf.pd.structure.PDStructTreeRoot;
import org.verapdf.tools.StaticResources;
import org.verapdf.wcag.algorithms.entities.geometry.BoundingBox;

import java.util.List;

/**
 * @author Maxim Plushchov
 */
/**
 * Adapts a PDF annotation and its owning page to the semantic-analysis model.
 *
 * <p>WCAG checks use annotation flags, contents, alternate text, crop-box status, and nearby
 * extracted text to assess whether an annotation has an accessible name. The factory specializes
 * link annotations while this class supplies common annotation properties; nearby text is found
 * lazily from page chunks intersecting the annotation rectangle.</p>
 *
 * <p><b>.NET porting notes:</b> Port as {@code VeraPdf.Validation.Wcag.Model.Impl.Sa.GFSAAnnotation}
 * implementing {@code VeraPdf.Model.SALayer.ISAAnnotation}. Use
 * {@code VeraPdf.Parser.Pd.PdfAnnotation} and {@code PdfPage}; preserve the lazy text lookup and
 * nullable parser values. The WCAG algorithm chunk store has no .NET port yet.</p>
 *
 * @author Maxim Plushchov
 */
public class GFSAAnnotation extends GenericModelObject implements SAAnnotation {
	public static final String ANNOTATION_TYPE = "SAAnnotation";

	/** Model link name used by profiles to reach link-specific annotation data. */
	public static final String LINK = "Link";

	private final PDAnnotation annot;
	private final PDPage page;
	private String textValue;

	/** Creates a generic annotation adapter using the standard SA model type. */
	public GFSAAnnotation(PDAnnotation annot, PDPage page) {
		this(annot, page, ANNOTATION_TYPE);
	}

	/** Creates an annotation adapter with a subtype-specific model type name. */
	public GFSAAnnotation(PDAnnotation annot, PDPage page, String type) {
		super(type);
		this.page = page;
		this.annot = annot;
	}

	@Override
	/** Returns text found inside the annotation rectangle, computing and caching it on demand. */
	public String gettextValue() {
		if (textValue == null) {
			textValue = findTextValue();
		}
		return textValue;
	}

	/** Concatenates extracted page text overlapping the annotation's PDF rectangle. */
	private String findTextValue() {
		double[] rect = annot.getRect();
		if (rect != null) {
			List<String> values = StaticStorages.getChunks().getValues(page.getObject().getKey(),
			                                                           new BoundingBox(page.getPageNumber(), rect));
			return values.stream()
			             .reduce("", String::concat)
			             .trim();
		}
		return "";
	}

	/** Selects the link-specific adapter when the PDF subtype is {@code Link}. */
	public static GFSAAnnotation createAnnot(PDAnnotation annot, PDPage page) {
		ASAtom subtype = annot.getSubtype();
		if (subtype == null) {
			return new GFSAAnnotation(annot, page);
		}
		String subtypeString = subtype.getValue();
		switch (subtypeString) {
			case LINK:
				return new GFSALinkAnnotation(annot, page);
			default:
				return new GFSAAnnotation(annot, page);
		}
	}

	@Override
	public Boolean getisOutsideCropBox() {
        return PDAnnotation.isOutsideCropBox(page, annot);
	}

	@Override
	public String getContents() {
		return annot.getContents();
	}

	@Override
    public String getAlt() {
        return annot.getAlt();
    }

	@Override
	public Long getF() {
		return annot.getF();
	}
}
