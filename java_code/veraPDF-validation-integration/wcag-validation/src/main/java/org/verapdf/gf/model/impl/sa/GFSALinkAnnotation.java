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

import org.verapdf.model.salayer.SALinkAnnotation;
import org.verapdf.pd.PDAnnotation;
import org.verapdf.pd.PDPage;

import java.net.MalformedURLException;
import java.net.URL;

/**
 * Adds URL-like checks to the common SA annotation adapter for link annotations.
 *
 * <p>PDF link annotations can expose a destination as visible contents, alternate text, or
 * extracted nearby text. The three model properties below let accessibility rules test which
 * value actually looks like a URL; malformed or non-absolute URL text is treated as false.</p>
 *
 * <p><b>.NET porting notes:</b> Port as
 * {@code VeraPdf.Validation.Wcag.Model.Impl.Sa.GFSALinkAnnotation}, implementing
 * {@code VeraPdf.Model.SALayer.ISALinkAnnotation}. Use {@code Uri.TryCreate} with an explicit
 * URI-kind policy rather than throwing construction, and preserve this adapter's false-on-invalid
 * behavior.</p>
 *
 * @author Maxim Plushchov
 */
public class GFSALinkAnnotation extends GFSAAnnotation implements SALinkAnnotation {

	/** Stable model type name for links, distinct from generic annotations. */
	public static final String LINK_ANNOTATION_TYPE = "SALinkAnnotation";

	public GFSALinkAnnotation(PDAnnotation annot, PDPage page) {
		super(annot, page, LINK_ANNOTATION_TYPE);
	}

	@Override
	public Boolean getcontentsIsLink() {
		return isLink(getContents());
	}

	@Override
	public Boolean getaltIsLink() {
		return isLink(getAlt());
	}

	@Override
	public Boolean gettextValueIsLink() {
		return isLink(gettextValue());
	}

	/** Returns whether text parses as a URL; invalid text is not an exceptional rule result. */
	private static Boolean isLink(String textValue) {
		try {
			new URL(textValue);
			return true;
		} catch (MalformedURLException ignored) {
		}
		return false;
	}

}
